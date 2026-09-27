using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cupid.Harness.Rules;

// HA11 - Records are Value Objects, never entities: a record is allowed under
// OmniProduct_CoreDomain.Models.* like any other type, but it must not implement IDentifiable -
// identity belongs to entities (HA9), and a record's job is to be a Value Object (immutable,
// equality by value, no identity). HA2 already forces "no setter" and HA5 already caps public
// properties at 4, so this rule only adds the one thing they don't check: record != entity.
//
// It also pins the workshop's running example: `Price` must exist as a record Value Object under
// OmniProduct_CoreDomain.Models - that's the concrete deliverable CUPID-step1-2.md asks for
// (extract VAT out of Price, keep it composable and immutable).
public sealed class RecordsAreValueObjectsRule : IHarnessRule
{
    private const string EntityNamespacePrefix = "OmniProduct_CoreDomain.Models";
    private const string EntityMarkerInterfaceName = "IDentifiable";
    private const string RequiredValueObjectTypeName = "Price";

    public string Id => "HA11";
    public string Name => "Records are Value Objects (never entities); 'Price' must be one";

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation)
    {
        var violations = new List<Violation>();

        var foundRequiredValueObject = false;

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);

            foreach (var recordDecl in tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                var typeSymbol = model.GetDeclaredSymbol(recordDecl) as ITypeSymbol;
                if (typeSymbol == null)
                    continue;

                if (typeSymbol.AllInterfaces.Any(i => i.Name == EntityMarkerInterfaceName))
                {
                    violations.Add(new Violation(
                        tree.FilePath,
                        recordDecl.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        $"'{recordDecl.Identifier.Text}' is a record that implements {EntityMarkerInterfaceName}. Records are Value Objects, not entities - drop {EntityMarkerInterfaceName} or turn this into a class that follows HA9/HA10 instead."));
                }

                if (recordDecl.Identifier.Text == RequiredValueObjectTypeName && IsInEntityNamespace(typeSymbol))
                {
                    foundRequiredValueObject = true;
                }
            }
        }

        if (!foundRequiredValueObject)
        {
            violations.Add(new Violation(
                trees.FirstOrDefault()?.FilePath ?? string.Empty,
                1,
                $"No record named '{RequiredValueObjectTypeName}' found under {EntityNamespacePrefix}.*. '{RequiredValueObjectTypeName}' must be a record Value Object (see CUPID-step1-2.md)."));
        }

        return violations;
    }

    private static bool IsInEntityNamespace(ITypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace?.ToDisplayString();
        return ns != null
               && (ns == EntityNamespacePrefix || ns.StartsWith(EntityNamespacePrefix + ".", StringComparison.Ordinal));
    }
}
