using System;

namespace FizzBuzzGame
{
	/// <summary>
	/// Basic interface for FizzBuzz rules
	/// </summary>
	public interface IRule : IComparable<IRule>
	{
		string Evaluate(int number);
		int Priority { get; }
		bool Final { get; }
	}
}

