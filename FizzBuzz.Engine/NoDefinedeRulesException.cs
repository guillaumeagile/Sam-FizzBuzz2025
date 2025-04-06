using System;

namespace FizzBuzzGame
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

