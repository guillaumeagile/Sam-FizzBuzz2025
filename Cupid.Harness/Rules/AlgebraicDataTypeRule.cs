using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA3 - an ADT is represented by a OneOf<T...> union, giving consumers exhaustive matching
// without introducing a record inheritance hierarchy.
public sealed class AlgebraicDataTypeRule : IHarnessRule
{
    public string Id => "HA3";
    public string Name => "ADT via OneOf union";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var hasOneOfUnion = trees.Any(tree =>
        {
            var semanticModel = compilation.GetSemanticModel(tree);
            return tree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>()
                .Any(typeName => IsOneOfUnion(typeName, semanticModel));
        });

        if (hasOneOfUnion)
            return Array.Empty<Violation>();

        return
        [
            new Violation(
                FilePath: trees.FirstOrDefault()?.FilePath ?? "<no files>",
                Line: 1,
                Message: "No OneOf ADT found. HA3 requires a OneOf<T0, T1, ...> union with at least two alternatives.")
        ];
    }

    private static bool IsOneOfUnion(GenericNameSyntax typeName, SemanticModel semanticModel)
    {
        if (typeName.Identifier.Text != "OneOf" || typeName.TypeArgumentList.Arguments.Count < 2)
            return false;

        var symbol = semanticModel.GetSymbolInfo(typeName).Symbol as INamedTypeSymbol;
        return symbol?.ContainingNamespace.ToDisplayString() == "OneOf"
            && symbol.ContainingAssembly.Name == "OneOf"
            && symbol.TypeArguments.Length >= 2;
    }
}
