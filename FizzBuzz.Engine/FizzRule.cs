namespace FizzBuzz.Engine
{
	public class FizzRule : BaseRule
	{
		public FizzRule() => base.Priority = 1;

		public override string Evaluate(int number) => IsFizz(number) ? "Fizz" : "";
	}
}

