﻿using FizzBuzz.Engine;
using FluentAssertions;

namespace FizzBuzz.Tests
{
    [TestFixture]
    public class FizzBuzzEngineTests
    {
        private FizzBuzzEngine engine;

        [SetUp]
        public void setUp()
        {
            List<IRule>? _rules = new List<IRule>();
            _rules.Add(new FizzRule());
            _rules.Add(new BuzzRule());
            _rules.Add(new BangRule());
            _rules.Add(new TheAnswerRule());

            engine = new FizzBuzzEngine(_rules);
        }

        [Test]
        public void CanSayOne()
        {
            // Act
            var one = engine.Say(1);

            // Assert
            one.Should().Be("1");
        }

        [Test]
        public void CanSayFizz()
        {
            // Act
            var fizz = engine.Say(3);

            // Assert
            fizz.Should().Be("Fizz");
        }

        [Test]
        public void CanStillSayFizz()
        {
            // Act
            var fizz = engine.Say(6);

            // Assert
            fizz.Should().Be("Fizz");
        }

        [Test]
        public void CanSayBuzz()
        {
            // Act
            var buzz = engine.Say(5);

            // Assert
            buzz?.Should().Be("Buzz");
        }

        [Test]
        public void CanSayFizzBuzz()
        {
            // Act
            var fizzbuzz = engine.Say(15);

            // Assert
            fizzbuzz.Should().Be("FizzBuzz");
        }

        [Test]
        public void WillExplodeWhenNoRules()
        {
            // Arrange
            engine = new FizzBuzzEngine(null); // too much complexity

            // Act

            Action act = () => engine.Say(15);

            act.Should().Throw<NoDefinedeRulesException>();
            ;
        }

        [Test]
        public void CanSayBang()
        {
            // Act
            var bang = engine.Say(7);

            // Assert
            bang.Should().Be("Bang");
        }

        [Test]
        public void CanSayFizzBang()
        {
            // Act
            var fizzBang = engine.Say(21);

            // Assert
            fizzBang.Should().Be("FizzBang");
        }

        [Test]
        public void CanSayFizzBuzzBang()
        {
            // Act
            var fizzBuzzBang = engine.Say(105);

            // Assert
            fizzBuzzBang.Should().Be("FizzBuzzBang");
        }

        [Test]
        public void RuleInjectionOrderDoesntMatter()
        {
            // Arrange
            List<IRule>? _rules = new List<IRule>();
            _rules.Add(new BangRule());
            _rules.Add(new FizzRule());
            _rules.Add(new BuzzRule());

            engine = new FizzBuzzEngine(_rules);

            // Act
            var fizzBang = engine.Say(21);

            // Assert
            fizzBang.Should().Be("FizzBang");
        }

        [Test]
        public void CantAddTheSameRuleTwice()
        {
            // Arrange
            List<IRule>? _rules = new List<IRule>();
            _rules.Add(new BangRule());
            _rules.Add(new FizzRule());
            _rules.Add(new BuzzRule());
            _rules.Add(new BangRule());

            engine = new FizzBuzzEngine(_rules);

            // Act
            var fizzBang = engine.Say(21);

            // Assert
            fizzBang.Should().Be("FizzBang");
        }

        [Test]
        public void CanSayTheAnswerTo42()
        {
            // Act
            var theAnswer = engine.Say(42);

            // Assert
            theAnswer.Should().Be("The answer to the meaning of life, the universe, and everything");
        }
    }
}