using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA7 - Fan-out (Unix Philosophy proxy): applies only to ENTITIES - types declared under
// OmniProduct_CoreDomain.Models.* that implement IDentifiable (see HA9) - and counts the distinct
// entity types (same definition) that entity directly touches via fields, parameters, and local
// variables. An entity wiring together many unrelated entities is orchestrating too many concerns
// at once, even if HA5 (public property count) and HA6 (method-name vocabulary) don't catch it -
// e.g. an entity with few properties but a constructor pulling in five entity collaborators.
// Services, value objects, and any other non-entity class are out of scope for this rule entirely
// - a service is expected to wire together many collaborators; that's HA10's concern, not HA7's.
public sealed class FanOutRule : IHarnessRule
{
    private const string EntityNamespacePrefix = "OmniProduct_CoreDomain.Models";
    private const string EntityMarkerInterfaceName = "IDentifiable";

    private readonly int _maxDistinctDomainTypes;

    public FanOutRule(int maxDistinctDomainTypes = 3)
    {
        _maxDistinctDomainTypes = maxDistinctDomainTypes;
    }

    public string Id => "HA7";
    public string Name => $"Fan-out (an entity touches at most {_maxDistinctDomainTypes} distinct entity type(s))";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();

        var entityTypeNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var typeDecl in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var typeSymbol = model.GetDeclaredSymbol(typeDecl) as ITypeSymbol;

                if (typeSymbol != null && IsEntity(typeSymbol))
                {
                    entityTypeNames.Add(typeDecl.Identifier.Text);
                }
            }
        }

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var typeDecl in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (!entityTypeNames.Contains(typeDecl.Identifier.Text))
                    continue; // HA7 only applies to entities; services/value objects are out of scope.

                var referencedTypeNames = new SortedSet<string>(StringComparer.Ordinal);

                foreach (var identifier in typeDecl.DescendantNodes().OfType<IdentifierNameSyntax>())
                {
                    var symbolInfo = model.GetSymbolInfo(identifier);
                    var typeSymbol = (symbolInfo.Symbol as ITypeSymbol)
                                      ?? (symbolInfo.Symbol as ILocalSymbol)?.Type
                                      ?? (symbolInfo.Symbol as IParameterSymbol)?.Type
                                      ?? (symbolInfo.Symbol as IFieldSymbol)?.Type;

                    if (typeSymbol != null && entityTypeNames.Contains(typeSymbol.Name) && typeSymbol.Name != typeDecl.Identifier.Text)
                    {
                        referencedTypeNames.Add(typeSymbol.Name);
                    }
                }

                if (referencedTypeNames.Count > _maxDistinctDomainTypes)
                {
                    violations.Add(new Violation(
                        tree.FilePath,
                        typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"'{typeDecl.Identifier.Text}' directly references {referencedTypeNames.Count} distinct entity types: {string.Join(", ", referencedTypeNames)}. Split responsibilities so each entity collaborates with fewer entities."));
                }
            }
        }

        return violations;
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
