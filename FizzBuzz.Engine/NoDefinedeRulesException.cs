namespace FizzBuzz.Engine
{
	public class NoDefinedeRulesException : ApplicationException
	{
		public NoDefinedeRulesException ()
		{
		}

		public NoDefinedeRulesException(string message) : base(message)
		{
		}
	}
}

