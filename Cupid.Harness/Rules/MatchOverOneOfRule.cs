using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA4.4 - Composable, step 4 (light): behaviour per alternative goes through OneOf.Match, never through type tests.
// Two checks:
//   1. no `is` / `as` / `switch` on a OneOf, nor testing for one of its alternatives (a type argument of any
//      OneOf<...> used in the source);
//   2. `.Match(...)` is actually called on at least one OneOf<Y, Z, ...> whose type arguments are all records.
public sealed class MatchOverOneOfRule : IHarnessRule
{
    public string Id => "HA4.4";
    public string Name => "Alternatives are handled with OneOf.Match over records, never is/as/switch";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();
        var models = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));
        var alternatives = CollectAlternatives(trees, models);
        var matchOverRecordsFound = false;

        foreach (var tree in trees)
        {
            var model = models[tree];

            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                var kind = TypeTestKind(node, model, alternatives);
                if (kind is not null)
                {
                    violations.Add(new Violation(
                        tree.FilePath,
                        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"'{kind}' on a OneOf or one of its alternatives. Handle each alternative with .Match(...) instead."));
                }
                else if (!matchOverRecordsFound && node is InvocationExpressionSyntax invocation)
                {
                    matchOverRecordsFound = IsMatchOverRecords(invocation, model);
                }
            }
        }

        if (!matchOverRecordsFound)
        {
            violations.Add(new Violation(
                trees.FirstOrDefault()?.FilePath ?? string.Empty,
                1,
                "No .Match(...) call on a OneOf<...> whose type arguments are all records. Handle the alternatives with Match."));
        }

        return violations;
    }

    private static string? TypeTestKind(SyntaxNode node, SemanticModel model, HashSet<ITypeSymbol> alternatives)
    {
        bool Tests(ExpressionSyntax operand, IEnumerable<TypeSyntax> testedTypes) =>
            IsOneOf(model.GetTypeInfo(operand).Type)
            || testedTypes.Any(t => model.GetTypeInfo(t).Type is { } type && alternatives.Contains(type));

        switch (node)
        {
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.IsExpression) || binary.IsKind(SyntaxKind.AsExpression):
                return Tests(binary.Left, binary.Right is TypeSyntax right ? [right] : []) ? (binary.IsKind(SyntaxKind.IsExpression) ? "is" : "as") : null;

            case IsPatternExpressionSyntax isPattern:
                return Tests(isPattern.Expression, PatternTypes(isPattern.Pattern)) ? "is" : null;

            case SwitchStatementSyntax switchStatement:
                return Tests(switchStatement.Expression, switchStatement.Sections
                    .SelectMany(s => s.Labels.OfType<CasePatternSwitchLabelSyntax>())
                    .SelectMany(l => PatternTypes(l.Pattern))) ? "switch" : null;

            case SwitchExpressionSyntax switchExpression:
                return Tests(switchExpression.GoverningExpression, switchExpression.Arms.SelectMany(a => PatternTypes(a.Pattern))) ? "switch" : null;

            default:
                return null;
        }
    }

    private static IEnumerable<TypeSyntax> PatternTypes(PatternSyntax pattern) =>
        pattern.DescendantNodesAndSelf().SelectMany<SyntaxNode, TypeSyntax>(n => n switch
        {
            DeclarationPatternSyntax d => [d.Type],
            TypePatternSyntax t => [t.Type],
            ConstantPatternSyntax { Expression: TypeSyntax name } => [name], // `Perishable => ...` parses as a constant pattern
            RecursivePatternSyntax { Type: { } type } => [type],
            _ => []
        });

    private static HashSet<ITypeSymbol> CollectAlternatives(IReadOnlyList<SyntaxTree> trees, Dictionary<SyntaxTree, SemanticModel> models)
    {
        var alternatives = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var tree in trees)
        {
            foreach (var generic in tree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>())
            {
                if (models[tree].GetSymbolInfo(generic).Symbol is INamedTypeSymbol type && IsOneOf(type))
                    alternatives.UnionWith(type.TypeArguments);
            }
        }

        return alternatives;
    }

    // Resolved from the receiver type, not the method symbol: the latter is null when the OneOf assembly's own
    // references (netstandard) are not part of the compilation.
    private static bool IsMatchOverRecords(InvocationExpressionSyntax invocation, SemanticModel model) =>
        invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Match" } access
        && model.GetTypeInfo(access.Expression).Type is INamedTypeSymbol receiver
        && IsOneOf(receiver)
        && receiver.TypeArguments.All(a => a.IsRecord);

    private static bool IsOneOf(ITypeSymbol? type) =>
        type is INamedTypeSymbol { IsGenericType: true } named
        && named.OriginalDefinition.Name == "OneOf"
        && named.OriginalDefinition.ContainingNamespace?.ToDisplayString() == "OneOf";
}
