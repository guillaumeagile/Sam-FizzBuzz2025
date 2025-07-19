namespace FizzBuzz.Engine
{
	public class TheAnswerRule : BaseRule
	{
		public TheAnswerRule() => base.Priority = 4;

		public override string Evaluate(int number)
		{
			return number == 42 ? "The answer to the meaning of life, the universe, and everything" : "";
		}

		public override bool Final => true;
	}
}

