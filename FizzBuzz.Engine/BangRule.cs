namespace FizzBuzz.Engine
{
	public class BangRule : BaseRule
	{
		public BangRule()
		{
			base._priority = 3;
		}

		public override string Evaluate(int number)
		{
			return IsBang(number) ? "Bang" : "";
		}
	}
}