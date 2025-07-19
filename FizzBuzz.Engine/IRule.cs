namespace FizzBuzz.Engine
{
	/// <summary>
	/// Basic interface for FizzBuzz rules
	/// </summary>
	public interface IRule
	{
		string Evaluate(int number);
		bool Final { get; }
	}
}
