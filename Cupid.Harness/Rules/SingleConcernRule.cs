using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA6 - Single Concern (Unix Philosophy: "do one thing well"). Groups a class's public method
// names by the bounded-context concern they belong to (see ubiquitous-language-map.json). A class
// whose public methods span more than one concern is mixing responsibilities - exactly the
// ProductService/Product smell from CUPID-step1-1's Concept 1.1 ("catalog, pricing, stock,
// supplier notification, and transport - all at once").
public sealed class SingleConcernRule : IHarnessRule
{
    private readonly UbiquitousLanguageMap _map;
    private readonly int _maxConcernsPerClass;

    public SingleConcernRule(UbiquitousLanguageMap map, int maxConcernsPerClass = 1)
    {
        _map = map;
        _maxConcernsPerClass = maxConcernsPerClass;
    }

    public string Id => "HA6";
    public string Name => $"Single Concern (public methods span at most {_maxConcernsPerClass} bounded-context concern(s))";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();

        foreach (var tree in trees)
        {
            var root = tree.GetRoot();

            var typeDecls = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(t => t is ClassDeclarationSyntax or RecordDeclarationSyntax);

            foreach (var typeDecl in typeDecls)
            {
                var publicMethods = typeDecl.Members
                    .OfType<MethodDeclarationSyntax>()
                    .Where(m => m.Modifiers.Any(mod => mod.Text == "public"))
                    .ToList();

                var methodsByConcern = publicMethods
                    .SelectMany(m => _map.ConcernsFor(m.Identifier.Text).Select(concern => (Concern: concern, Method: m)))
                    .ToList();

                var distinctConcerns = methodsByConcern
                    .Select(x => x.Concern)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(c => c, StringComparer.Ordinal)
                    .ToList();

                if (distinctConcerns.Count > _maxConcernsPerClass)
                {
                    var detail = string.Join(", ", distinctConcerns.Select(concern =>
                        $"{concern} ({string.Join("/", methodsByConcern.Where(x => x.Concern == concern).Select(x => x.Method.Identifier.Text))})"));

                    violations.Add(new Violation(
                        tree.FilePath,
                        typeDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"'{typeDecl.Identifier.Text}' mixes {distinctConcerns.Count} concerns: {detail}. Extract one class per concern."));
                }
            }
        }

        return violations;
    }
}
