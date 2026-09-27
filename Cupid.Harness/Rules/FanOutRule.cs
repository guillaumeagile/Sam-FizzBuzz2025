using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA7 - Fan-out (Unix Philosophy proxy): counts the distinct ENTITY types (types declared in the
// analyzed sources that implement IDentifiable, not every domain type, and not BCL/primitives)
// that a class directly touches via fields, parameters, and local variables. A class wiring
// together many unrelated entities is orchestrating too many concerns at once, even if HA5
// (public property count) and HA6 (method-name vocabulary) don't catch it - e.g. a class with few
// properties but a constructor pulling in five entity collaborators. Value objects, services, and
// other non-entity domain types don't count - only types that opt into identity via IDentifiable.
public sealed class FanOutRule : IHarnessRule
{
    private const string EntityMarkerInterfaceName = "IDentifiable";

    private readonly int _maxDistinctDomainTypes;

    public FanOutRule(int maxDistinctDomainTypes = 3)
    {
        _maxDistinctDomainTypes = maxDistinctDomainTypes;
    }

    public string Id => "HA7";
    public string Name => $"Fan-out (a class touches at most {_maxDistinctDomainTypes} distinct entity type(s))";

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

                if (typeSymbol != null && ImplementsEntityMarker(typeSymbol))
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
                        $"'{typeDecl.Identifier.Text}' directly references {referencedTypeNames.Count} distinct entity types: {string.Join(", ", referencedTypeNames)}. Split responsibilities so each class collaborates with fewer entities."));
                }
            }
        }

        return violations;
    }

    private static bool ImplementsEntityMarker(ITypeSymbol typeSymbol)
    {
        return typeSymbol.AllInterfaces.Any(i => i.Name == EntityMarkerInterfaceName);
    }
}
