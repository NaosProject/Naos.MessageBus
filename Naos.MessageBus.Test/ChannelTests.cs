// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.MessageBus.Domain;

    using Xunit;

    public class ChannelTests
    {
        [Fact]
        public void DisinctOnChannelCollection_Duplicates_ReturnsActualDistinct()
        {
            // arrange
            var duplicateName = "HelloDolly";
            var input = new[] { new Channel(duplicateName), new Channel(duplicateName) };

            // act
            var actual = input.Distinct().ToList();

            // assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(duplicateName, actual.Single().Name);
        }

        [Fact]
        public void CompareTo_NullInput_Throws()
        {
            // arrange
            var first = new Channel("MonkeysRock");
            Action testCode = () => first.CompareTo(null);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot compare a null channel.");
        }

        [Fact]
        public void CompareTo_SameName_Zero()
        {
            // arrange
            var name = "MonkeysRock";
            var first = new Channel(name);
            var second = new Channel(name);

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public void CompareTo_DifferentNameHigh_NegativeOne()
        {
            // arrange
            var first = new Channel("b");
            var second = new Channel("a");

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(1, actual);
        }

        [Fact]
        public void CompareTo_DifferentNameLow_One()
        {
            // arrange
            var first = new Channel("1");
            var second = new Channel("2");

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void Equals_NullFirst_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel("someName");
            Action testCode = () => notReallyNeeded.Equals(new Channel("someOtherName"), null);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot compare null channels.");
        }

        [Fact]
        public void Equals_NullSecond_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel("someName");
            Action testCode = () => notReallyNeeded.Equals(null, new Channel("someOtherName"));

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot compare null channels.");
        }

        [Fact]
        public void Equals_NullBoth_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel("someName");
            Action testCode = () => notReallyNeeded.Equals(null, null);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot compare null channels.");
        }

        [Fact]
        public void InstanceEquals_Null_Throws()
        {
            // arrange
            var first = new Channel("someName");
            Action testCode = () => first.Equals(null);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot compare a null channel.");
        }

        [Fact]
        public void InstanceEquals_AreNotEqual_False()
        {
            // arrange
            var first = new Channel("asdf");
            var second = new Channel("something else");

            // act
            var actual = first.Equals(second);

            // assert
            Assert.Equal(false, actual);
        }

        [Fact]
        public void InstanceEquals_AreEqual_True()
        {
            // arrange
            var name = "asdf2";
            var first = new Channel(name);
            var second = new Channel(name);

            // act
            var actual = first.Equals(second);

            // assert
            Assert.Equal(true, actual);
        }
    }
}
