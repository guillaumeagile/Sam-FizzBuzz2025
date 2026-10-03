using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA7 - Fan-out (Unix Philosophy proxy), strict form: applies only to ENTITIES - types declared
// under OmniProduct_CoreDomain.Models.* that implement IDentifiable (see HA9) - and an entity may
// reference NO other entity type at all (default cap = 0). Entities relate to each other by
// identifier only (e.g. a WarehouseId), never by holding or accepting the other entity object:
// not via fields, properties, method/constructor parameters, return types, locals, or generic
// arguments (List<Supplier>, Dictionary<string, Supplier>, Supplier[], Supplier?).
// Entities are matched by symbol identity, not by simple name, so a same-named type in another
// namespace neither triggers nor masks a violation.
// Services, value objects, and any other non-entity class are out of scope for this rule entirely
// - a service is expected to wire together many collaborators; that's HA10's concern, not HA7's.
public sealed class FanOutRule : IHarnessRule
{
    private const string EntityNamespacePrefix = "OmniProduct_CoreDomain.Models";
    private const string EntityMarkerInterfaceName = "IDentifiable";

    private readonly int _maxDistinctDomainTypes;

    public FanOutRule(int maxDistinctDomainTypes = 0)
    {
        _maxDistinctDomainTypes = maxDistinctDomainTypes;
    }

    public string Id => "HA7";
    public string Name => $"Fan-out (an entity references at most {_maxDistinctDomainTypes} other entity type(s); reference by id instead)";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var typeDecl in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var declared = model.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (declared == null || !IsEntity(declared))
                    continue; // HA7 only applies to entities; services/value objects are out of scope.

                var referenced = new SortedSet<string>(StringComparer.Ordinal);

                foreach (var node in typeDecl.DescendantNodes())
                {
                    if (node is not (IdentifierNameSyntax or GenericNameSyntax))
                        continue;

                    var info = model.GetSymbolInfo(node);
                    var symbol = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
                    var type = symbol switch
                    {
                        ITypeSymbol t => t,
                        ILocalSymbol l => l.Type,
                        IParameterSymbol p => p.Type,
                        IFieldSymbol f => f.Type,
                        IPropertySymbol pr => pr.Type,
                        IMethodSymbol { MethodKind: MethodKind.Constructor } c => c.ContainingType,
                        _ => null
                    };

                    foreach (var entity in EntitiesIn(type))
                    {
                        if (!SymbolEqualityComparer.Default.Equals(entity, declared))
                            referenced.Add(entity.Name);
                    }
                }

                if (referenced.Count > _maxDistinctDomainTypes)
                {
                    violations.Add(new Violation(
                        tree.FilePath,
                        typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"'{typeDecl.Identifier.Text}' directly references {referenced.Count} distinct entity types: {string.Join(", ", referenced)}. Entities must not reference other entities - hold their identifier instead."));
                }
            }
        }

        return violations;
    }

    // Unwraps arrays, nullable and generic arguments so List<Supplier> counts as Supplier.
    private static IEnumerable<INamedTypeSymbol> EntitiesIn(ITypeSymbol? type)
    {
        switch (type)
        {
            case null:
                yield break;
            case IArrayTypeSymbol array:
                foreach (var e in EntitiesIn(array.ElementType)) yield return e;
                break;
            case INamedTypeSymbol named:
                if (IsEntity(named)) yield return named.OriginalDefinition;
                foreach (var arg in named.TypeArguments)
                    foreach (var e in EntitiesIn(arg)) yield return e;
                break;
        }
    }

    private static bool IsEntity(ITypeSymbol typeSymbol)
    {
        return IsInEntityNamespace(typeSymbol) && typeSymbol.AllInterfaces.Any(i => i.Name == EntityMarkerInterfaceName);
    }

    private static bool IsInEntityNamespace(ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace?.ToDisplayString();
        return ns != null
               && (ns == EntityNamespacePrefix || ns.StartsWith(EntityNamespacePrefix + ".", StringComparison.Ordinal));
    }
}
