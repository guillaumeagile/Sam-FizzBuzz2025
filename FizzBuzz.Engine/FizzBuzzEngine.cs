namespace FizzBuzz.Engine
{
	/// <summary>
	/// Fizz buzz game engine.
	/// </summary>
	public class FizzBuzzEngine
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FizzBuzzEngine"/> class.
		/// The basic engine should always be initialized with Fizz and Buzz rules.
		/// </summary>
		/// <param name="rules">Rules.</param>
		public FizzBuzzEngine()
		{
			_rules = new SortedSet<IRule> { new FizzRule(), new BuzzRule() };
		}
		
		public FizzBuzzEngine(IEnumerable<IRule>? rules)
		{
			_rules = new SortedSet<IRule> { new FizzRule(), new BuzzRule() };
			if (rules != null) {  
				_rules.Concat(rules).ToList().ForEach(rule => _rules.Add(rule));
			}
		}

		private SortedSet<IRule> _rules;	
		public void AddRule(IRule rule)
		{
			_rules.Add(rule);
		}
		
		private SortedSet<IRule> Rules 
		{
			get 
			{
				if (_rules == null || !_rules.Any())  // uncessary complexity
				{
					throw new NoDefinedeRulesException ("You must declare some rules to play");
				}
				return _rules;
			}
		}	

		/// <summary>
		/// Say the specified number after applying the rules.
		/// </summary>
		/// <param name="number">Number.</param>
		public object Say(int number)
		{
			var finalAnswer = "";
			foreach(var rule in Rules)
			{
				var answer = rule.Evaluate (number);
				if (answer != string.Empty && rule.Final) {
					finalAnswer = answer;
					break;
				}
				finalAnswer += answer;

			}
			return (finalAnswer != string.Empty) ? finalAnswer : number.ToString();
		}

	
	}
}

