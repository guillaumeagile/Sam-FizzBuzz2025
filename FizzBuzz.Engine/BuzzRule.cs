namespace FizzBuzz.Engine
{
	public class BuzzRule : BaseRule
	{
		public BuzzRule() => base.Priority = 2;

		public override string Evaluate(int number) => IsBuzz(number) ? "Buzz" : "";
	}
}

