using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FizzBuzzGame;


namespace FizzBuzzRunner
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			int start = 1;
			int count = 125;
			if (args.Length == 2) {
				try {
					start = int.Parse (args [0]);
					count = int.Parse (args [1]);
					Console.WriteLine(String.Format ("Ok, we're going to count from {0} to {1}", start, start + (count - 1))); 
				} catch (Exception e) {
					Console.WriteLine ("D'oh!, you entered non-integer values, you ID10T!. Default values for you :P");
					Console.WriteLine(e.Message);
					Console.WriteLine("--- end of rant --- on with the show.");
					Console.WriteLine("");
				}
			}

			List<IRule>? _rules = new List<IRule> ();
			_rules.Add (new FizzRule ());
			_rules.Add (new BuzzRule ());
			_rules.Add (new BangRule ());
			FizzBuzzEngine fb = new FizzBuzzEngine (_rules);
			foreach (int number in Enumerable.Range(start, count)) {
				Console.WriteLine (fb.Say (number));
			}
		}
	}
}
