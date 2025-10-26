namespace FizzBuzz.Engine.Rules.Abstractions
{
	/// <summary>
	/// Basic interface for FizzBuzz rules
	/// Simplified version - no Priority (removed for CUPID compliance)
	/// </summary>
	public interface IRule
	{
		string Evaluate(int number);
		bool? Final { get; }
	}
}


/* reminder: the full "ugly" SOLID version had:

/// <summary>
   /// Basic interface for FizzBuzz rules
   /// </summary>
   public interface IRule : IComparable<IRule>
   {
   	string Evaluate(int number);
   	int Priority { get; }  // <-- Removed for predictability
   	bool Final { get; }
   }

   */
