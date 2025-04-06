using System;
using System.Collections.Generic;
using System.Linq;

namespace FizzBuzzGame
{
	/// <summary>
	/// Fizz buzz game engine.
	/// </summary>
	public class FizzBuzzEngine
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FizzBuzzGame.FizzBuzzEngine"/> class.
		/// </summary>
		/// <param name="rules">Rules.</param>
		public FizzBuzzEngine(IEnumerable<IRule>? rules)
		{
			if (rules == null) {
				rules = new List<IRule> ();
			}
			_rules = new SortedSet<IRule> (rules);
		}

		private SortedSet<IRule> _rules;	
		/// <summary>
		/// Gets the rules.
		/// </summary>
		/// <remarks>
		/// Use a SortedSet so that the rules are sorted and unique.
		/// </remarks>
		/// <exception cref="FizzBuzzGame.NoDefinedRulesException">
		/// Verifies that there are some rules defined or throws this exception
		/// </exception>
		/// <value>The rules.</value>
		private SortedSet<IRule> Rules 
		{
			get 
			{
				if (_rules == null || !_rules.Any()) 
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

