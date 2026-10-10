using Microsoft.CodeAnalysis;

namespace Cupid.Harness.Rules;

// HA12.1 (warning) - primitive obsession on identifiers: an `Id` / `*Id` typed as anything but a record
// wrapper (string, int, a bare Guid/Ulid, ...) should be wrapped in a record such as
// record ProductId(Guid Value), so ids of different entities cannot be mixed up. HA12 is the error
// side (the type must be Guid/Ulid based); this rule only asks for the wrapper.
public sealed class BareIdentifierIsWeakRule : IHarnessRule
{
    public string Id => "HA12.1";
    public string Name => "Identifiers should be wrapped in a record, not a bare primitive/Guid/Ulid (warning)";
    public HarnessSeverity Severity => HarnessSeverity.Warning;

    public IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation) =>
        IdentifierShape.FindIdMembers(trees, compilation)
            .Where(m => !IdentifierShape.IsWrapper(m.Type))
            .Select(m =>
            {
                var type = m.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var wrapper = m.Name is "Id" or "id" ? m.Owner + "Id" : char.ToUpperInvariant(m.Name[0]) + m.Name[1..];
                return new Violation(m.Tree.FilePath, IdentifierShape.Line(m.Node),
                    $"'{m.Owner}.{m.Name}' is a bare {type}. Wrap it in a record, e.g. record {wrapper}(Guid Value).");
            })
            .ToList();
}
