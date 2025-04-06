using System;

namespace FizzBuzzGame
{
	public class TheAnswerRule : BaseRule
	{
		public TheAnswerRule()
		{
			base._priority = 4;
		}

		public override string Evaluate(int number)
		{
			return number == 42 ? "The answer to the meaning of life, the universe, and everything" : "";
		}

		public override bool Final { get{ return true; }}
	}
}

