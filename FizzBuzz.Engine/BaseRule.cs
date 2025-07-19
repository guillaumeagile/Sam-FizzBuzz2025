namespace FizzBuzz.Engine
{
	/// <summary>
	/// Base rule abstract class for creating new rules
	/// TERRIBLE OO DESIGN
	/// ask Claude the question:
	///  does BaseRule respect the SOLID principles ?
	/// </summary>
	public abstract class BaseRule : IRule
	{
		public virtual string Evaluate(int number) => number.ToString ();

		public int Priority { get; protected set; }

		public virtual bool Final => false;
		
		public int CompareTo(IRule? other)  
		{
			if (other == null)
				return 1;
			return this.Priority.CompareTo (other.Priority);
		}

		protected static bool IsFizz(int number) => IsMultipleOf(number, 3);

		protected static bool IsBuzz(int number) => IsMultipleOf(number, 5);

		protected static bool IsBang(int number) => IsMultipleOf(number, 7);

		private static bool IsMultipleOf(int number, int divisor)
		{
			return number % divisor == 0;
		}
	}
}

