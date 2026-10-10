using Microsoft.CodeAnalysis;

namespace Cupid.Harness.Rules;

// HA12.1 (warning) - a bare Guid/Ulid `Id` is acceptable for HA12 but is still primitive obsession:
// wrap it in a record (record ProductId(Guid Value)) so ids of different entities cannot be mixed up.
public sealed class BareIdentifierIsWeakRule : IHarnessRule
{
    public string Id => "HA12.1";
    public string Name => "Identifiers should be wrapped in a record, not a bare Guid/Ulid (warning)";
    public HarnessSeverity Severity => HarnessSeverity.Warning;

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation) =>
        IdentifierShape.FindIdMembers(trees, compilation)
            .Where(m => IdentifierShape.IsGuidOrUlid(m.Type) && !IdentifierShape.IsWrapper(m.Type))
            .Select(m => new Violation(m.Tree.FilePath, IdentifierShape.Line(m.Node),
                $"'{m.Owner}.{m.Name}' is a bare {m.Type.Name}. Wrap it, e.g. record {char.ToUpperInvariant(m.Name[0])}{m.Name[1..]}({m.Type.Name} Value)."))
            .ToList();
}
