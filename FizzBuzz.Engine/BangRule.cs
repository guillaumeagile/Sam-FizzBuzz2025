namespace FizzBuzz.Engine
{
	public class BangRule : BaseRule
	{
		public BangRule() => base.Priority = 3;

		public override string Evaluate(int number) => IsBang(number) ? "Bang" : "";
	}
}