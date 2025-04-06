using System;

namespace FizzBuzzGame
{
	public class BuzzRule : BaseRule
	{
		public BuzzRule()
		{
			base._priority = 2;
		}

		public override string Evaluate(int number)
		{
			return IsBuzz(number) ? "Buzz" : "";
		}
	}
}

