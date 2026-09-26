using Microsoft.CodeAnalysis;

namespace Cupid.Harness.Rules;

public interface IHarnessRule
{
    string Id { get; }
    string Name { get; }

    IReadOnlyList<Violation> Check(IReadOnlyList<SyntaxTree> trees, Compilation compilation);
}

public sealed record Violation(string FilePath, int Line, string Message);
