namespace FizzBuzz.Engine
{
	/// <summary>
	/// Base rule abstract class for creating new rules
	/// </summary>
	public abstract class BaseRule : IRule
	{
		public virtual string Evaluate(int number)
		{
			return number.ToString ();
		}

		protected int _priority;
		public int Priority { get { return _priority; } }

		public virtual bool Final { get { return false; } }

		/// pointless... the best documentation is the code itself
		/// <summary>
		/// Compares two rules by priority, from 1 to n
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo(IRule other)   // here we need nullable !
		{
			return this.Priority.CompareTo (other.Priority);
		}

		protected static bool IsFizz(int number)
		{
			return IsMultipleOf(number, 3);
		}

		protected static bool IsBuzz(int number)
		{
			return IsMultipleOf(number, 5);
		}

		protected static bool IsBang(int number)
		{
			return IsMultipleOf(number, 7);
		}

		protected static bool IsMultipleOf(int number, int divisor)
		{
			return number % divisor == 0;
		}
	}
}

