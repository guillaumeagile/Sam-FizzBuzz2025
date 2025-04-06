using System;

namespace FizzBuzzGame
{
	public class FizzRule : BaseRule
	{
		public FizzRule()
		{
			base._priority = 1;
		}

		public override string Evaluate(int number)
		{
			return IsFizz(number) ? "Fizz" : "";
		}
	}
}

