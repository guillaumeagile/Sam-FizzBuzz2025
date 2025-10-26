using FizzBuzz.Engine.Rules.Result;

namespace FizzBuzz.Engine.Rules.Abstractions
{
	/// <summary>
	/// A rule is simply a function that returns either a continuation or a final result
	/// This eliminates the need for separate Evaluate and Final properties
	/// </summary>
	public interface IRule
	{
		RuleResult Evaluate(int number);
	}
}
