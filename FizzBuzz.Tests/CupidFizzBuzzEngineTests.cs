using FizzBuzz.Engine;
using FluentAssertions;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class CupidFizzBuzzEngineTests
    {
        [Test]
        [Category("StandardEngine")]
        public void StandardEngine_ShouldHandleBasicNumbers_DefaultRuleSet()
        {
            // Arrange
            var engine = CupidFizzBuzzEngine.Standard();

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(15).Should().Be("FizzBuzz");
        }

        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_DefaultRuleSet_WhatHappenIfWeInsertTHeSameRuleTwice()
        {
            // Arrange
            var extendedRules = new List<IRule>() { FizzBuzzRules.Buzz() };
            var engine = CupidFizzBuzzEngine.Extended(extendedRules );

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(15).Should().Be("FizzBuzz");
        }

        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_DefaultRuleSet_WhatHappenIfWeInsertTHeSameRuleTwice_2()
        {
            // Arrange
            var extendedRules = new List<IRule>() { FizzBuzzRules.Buzz(), FizzBuzzRules.Fizz() };
            var engine = CupidFizzBuzzEngine.Extended(extendedRules );

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(15).Should().Be("FizzBuzz");
        }


        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_AndOneExtendedRule()
        {
            // Arrange

            var extendedRules = new List<IRule>() { FizzBuzzRules.Bang() };
            var engine = CupidFizzBuzzEngine.Extended(extendedRules );

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(7).Should().Be("Bang");
            engine.Evaluate(15).Should().Be("FizzBuzz");
            engine.Evaluate(21).Should().Be("FizzBang");
            engine.Evaluate(105).Should().Be("FizzBuzzBang");
//            engine.Evaluate(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_AndOneExtendedRule_AndRepeatAnExisting()
        {
            // Arrange

            var extendedRules = new List<IRule>() { FizzBuzzRules.Bang() , FizzBuzzRules.Fizz()};
            var engine = CupidFizzBuzzEngine.Extended(extendedRules );

            engine.Rules.Count.Should().Be(3);

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(7).Should().Be("Bang");
            engine.Evaluate(15).Should().Be("FizzBuzz");
            engine.Evaluate(21).Should().Be("FizzBang");
            engine.Evaluate(105).Should().Be("FizzBuzzBang");
//            engine.Evaluate(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }


        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_AndTwoExtendedRules()
        {
            // Arrange

            var extendedRules = new List<IRule>() {   FizzBuzzRules.Bang() , FizzBuzzRules.TheAnswer()};
            var engine = CupidFizzBuzzEngine.Extended(extendedRules );

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(3).Should().Be("Fizz");
            engine.Evaluate(5).Should().Be("Buzz");
            engine.Evaluate(7).Should().Be("Bang");
            engine.Evaluate(15).Should().Be("FizzBuzz");
            engine.Evaluate(21).Should().Be("FizzBang");
            engine.Evaluate(105).Should().Be("FizzBuzzBang");

       engine.Evaluate(42).Should().Be("The answer to the meaning of life, the universe, and everything");
        }

        [Test]
        [Category("ExtendedEngine")]
        public void ExtendedEngine_ShouldHandleBasicNumbers_OrderOfRulesMatters()
        {
            // Arrange

            var extendedRules = new List<IRule>()
            {
                FizzBuzzRules.Bang() ,
                FizzBuzzRules.Meeeh()
            };
            var engine = CupidFizzBuzzEngine.Extended(extendedRules);

            engine.Evaluate(21).Should().Be("FizzBangMeeeh");
        }

        [Test]
        [Category("CustomEngine")]
        public void CustomEngine_ShouldHandleBasicNumbers_OrderOfRulesMatters()
        {
            // Arrange

            var extendedRules = new List<IRule>()
            {
                FizzBuzzRules.Bang() ,
                FizzBuzzRules.Meeeh()
            };
            var engine =  CupidFizzBuzzEngine.NewSet(extendedRules);

            engine.Evaluate(21).Should().Be("BangMeeeh");
        }

        [Test]
        [Category("CustomEngine")]
        public void CustomEngine_ShouldHandleBasicNumbers_OrderOfRulesMatters_DifferentOrder()
        {
            // Arrange
            var extendedRules = new List<IRule>()
            {
                FizzBuzzRules.Meeeh(),
                FizzBuzzRules.Bang()
            };
            var engine = CupidFizzBuzzEngine.NewSet(extendedRules);

            engine.Evaluate(21).Should().Be("MeeehBang");
        }


        [Test]
        [Category("CustomEngine")]
        public void CustomEngine_ShouldBeComposableWithDomainRules()
        {
            // Arrange - Domain-focused rule creation
            var rules = new IRule[]
            {
                FizzBuzzRules.Divisible(by: 2, output: "Even"),
                FizzBuzzRules.Divisible(by: 4, output: "Quad")
            };

            var engine = new CupidFizzBuzzEngine(rules);

            // Act & Assert
            engine.Evaluate(1).Should().Be("1");
            engine.Evaluate(2).Should().Be("Even");
            engine.Evaluate(4).Should().Be("EvenQuad");
            engine.Evaluate(8).Should().Be("EvenQuad");
        }

        [Test]
        public void ExactMatchRule_ShouldStopProcessing()
        {
            // Arrange - Order matters: ExactMatchRule first will override Fizz
            var rules = new IRule[]
            {
                new ExactMatchRule(3, "Special Three"), // First = higher priority
                FizzBuzzRules.Fizz()
            };

            var engine = new CupidFizzBuzzEngine(rules);

            // Act & Assert
            engine.Evaluate(3).Should().Be("Special Three"); // Not "Fizz"
            engine.Evaluate(6).Should().Be("Fizz"); // Normal fizz rule
        }

        [Test]
        public void ExactMatchRule_ShouldStopProcessing_OrderOfRulesDoesntMatter()
        {
            // Arrange - Order matters: ExactMatchRule first will override Fizz
            var rules = new IRule[]
            {
                FizzBuzzRules.Fizz(),
                new ExactMatchRule(3, "Special Three")

            };

            var engine = new CupidFizzBuzzEngine(rules);

            // Act & Assert
            engine.Evaluate(3).Should().Be("Special Three"); // Not "Fizz"
            engine.Evaluate(6).Should().Be("Fizz"); // Normal fizz rule
        }



        //TODO : ajouter des tests dédiés à RuleResult


        [Test]
        public void RuleResult_EitherMonad_ShouldWorkCorrectly()
        {
            // Arrange - Test the Either monad pattern directly
            var continueRule = FizzBuzzRules.Fizz();
            var finalRule = new ExactMatchRule(42, "Final Answer");

            // Act & Assert - Continue case
            var continueResult = continueRule.Evaluate(3);
            continueResult.Should().BeOfType<RuleResult.Continue>();
            ((RuleResult.Continue)continueResult).Output.Should().Be("Fizz");

            var emptyResult = continueRule.Evaluate(4);
            emptyResult.Should().BeOfType<RuleResult.Continue>();
            ((RuleResult.Continue)emptyResult).Output.Should().Be(string.Empty);

            // Act & Assert - Final case
            var finalResult = finalRule.Evaluate(42);
            finalResult.Should().BeOfType<RuleResult.Final>();
            ((RuleResult.Final)finalResult).Output.Should().Be("Final Answer");

            var noMatchResult = finalRule.Evaluate(41);
            noMatchResult.Should().BeOfType<RuleResult.Continue>();
        }
    }
}
