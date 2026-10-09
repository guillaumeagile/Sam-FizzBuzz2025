using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA4.1 - Composable, step 1: composition needs value objects, and they must be immutable.
// A value object is a record / record struct that does not implement IDentifiable (identity belongs
// to entities, see HA9/HA11). No type name or namespace is pinned: at least one such record must exist,
// and every one of them must have no setter, no mutable field and no mutable collection.
public sealed class ValueObjectsAreImmutableRule : IHarnessRule
{
    private const string EntityMarkerInterfaceName = "IDentifiable";

    private static readonly HashSet<string> MutableCollectionTypes = new(StringComparer.Ordinal)
    {
        "List", "Dictionary", "HashSet", "Queue", "Stack", "LinkedList", "SortedList", "SortedDictionary"
    };

    public string Id => "HA4.1";
    public string Name => "Value objects exist and are immutable (records that are not IDentifiable)";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();
        var valueObjectCount = 0;

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var recordDecl in tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(recordDecl) is not INamedTypeSymbol symbol || !IsValueObject(symbol))
                    continue;

                valueObjectCount++;
                violations.AddRange(CheckImmutable(tree, recordDecl, symbol));
            }
        }

        if (valueObjectCount == 0)
        {
            violations.Add(new Violation(
                trees.FirstOrDefault()?.FilePath ?? string.Empty,
                1,
                $"No value object found. Declare at least one immutable record (or record struct) that does not implement {EntityMarkerInterfaceName}."));
        }

        return violations;
    }

    // Shared definition of "value object" so HA4.2 can reuse it.
    internal static bool IsValueObject(INamedTypeSymbol symbol) =>
        symbol.IsRecord && symbol.AllInterfaces.All(i => i.Name != EntityMarkerInterfaceName);

    private static IEnumerable<Violation> CheckImmutable(SyntaxTree tree, RecordDeclarationSyntax recordDecl, INamedTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            switch (member)
            {
                case IPropertySymbol { IsStatic: false } property:
                    if (property.SetMethod is { IsInitOnly: false })
                        yield return At(tree, recordDecl, $"Value object '{symbol.Name}' has a mutable 'set' on '{property.Name}'. Use 'init' or make it read-only.");
                    else if (IsMutableCollection(property.Type))
                        yield return At(tree, recordDecl, $"Value object '{symbol.Name}' exposes mutable collection '{property.Name}'. Use an immutable collection.");
                    break;

                case IFieldSymbol { IsStatic: false, IsReadOnly: false, IsImplicitlyDeclared: false } field:
                    yield return At(tree, recordDecl, $"Value object '{symbol.Name}' has a mutable field '{field.Name}'. Make it readonly.");
                    break;
            }
        }
    }

    private static bool IsMutableCollection(ITypeSymbol type) =>
        type is INamedTypeSymbol { IsGenericType: true } named && MutableCollectionTypes.Contains(named.Name);

    private static Violation At(SyntaxTree tree, RecordDeclarationSyntax recordDecl, string message) =>
        new(tree.FilePath, recordDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1, message);
}
