namespace FizzBuzz.Engine
{
	/// <summary>
	/// Basic interface for FizzBuzz rules, what seems wrong here ?
	/// </summary>
	public interface IRule : IComparable<IRule>
	{
		string Evaluate(int number);
		int Priority { get; }
		bool Final { get; }
	}
}

// too much complexity, we can kick things down the road
