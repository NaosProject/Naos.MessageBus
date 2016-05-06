// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChannelTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using Naos.MessageBus.Domain;

    using Xunit;

    public class ChannelTests
    {
        [Fact]
        public void DisinctOnChannelCollection_Duplicates_ReturnsActualDistinct()
        {
            // arrange
            var duplicateName = "HelloDolly";
            var input = new[] { new Channel { Name = duplicateName }, new Channel { Name = duplicateName } };

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
            var first = new Channel { Name = "MonkeysRock" };
            
            // act
            var rawEx = Record.Exception(() => first.CompareTo(null));

            // assert
            var typedEx = Assert.IsType<ArgumentException>(rawEx);
            Assert.Equal("Cannot compare a null channel.", typedEx.Message);
        }

        [Fact]
        public void CompareTo_SameName_Zero()
        {
            // arrange
            var name = "MonkeysRock";
            var first = new Channel { Name = name };
            var second = new Channel { Name = name };

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public void CompareTo_DifferentNameHigh_NegativeOne()
        {
            // arrange
            var first = new Channel { Name = "b" };
            var second = new Channel { Name = "a" };

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(1, actual);
        }

        [Fact]
        public void CompareTo_DifferentNameLow_One()
        {
            // arrange
            var first = new Channel { Name = "1" };
            var second = new Channel { Name = "2" };

            // act
            var actual = first.CompareTo(second);

            // assert
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void Equals_NullFirst_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel();

            // act
            var rawEx = Record.Exception(() => notReallyNeeded.Equals(new Channel(), null));

            // assert
            var typedEx = Assert.IsType<ArgumentException>(rawEx);
            Assert.Equal("Cannot compare null channels.", typedEx.Message);
        }

        [Fact]
        public void Equals_NullSecond_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel();

            // act
            var rawEx = Record.Exception(() => notReallyNeeded.Equals(null, new Channel()));

            // assert
            var typedEx = Assert.IsType<ArgumentException>(rawEx);
            Assert.Equal("Cannot compare null channels.", typedEx.Message);
        }

        [Fact]
        public void Equals_NullBoth_Throws()
        {
            // arrange
            var notReallyNeeded = new Channel();

            // act
            var rawEx = Record.Exception(() => notReallyNeeded.Equals(null, null));

            // assert
            var typedEx = Assert.IsType<ArgumentException>(rawEx);
            Assert.Equal("Cannot compare null channels.", typedEx.Message);
        }

        [Fact]
        public void InstanceEquals_Null_Throws()
        {
            // arrange
            var first = new Channel();

            // act
            var rawEx = Record.Exception(() => first.Equals(null));

            // assert
            var typedEx = Assert.IsType<ArgumentException>(rawEx);
            Assert.Equal("Cannot compare a null channel.", typedEx.Message);
        }

        [Fact]
        public void InstanceEquals_AreNotEqual_False()
        {
            // arrange
            var first = new Channel { Name = "asdf" };
            var second = new Channel { Name = "something else" };

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
            var first = new Channel { Name = name };
            var second = new Channel { Name = name };

            // act
            var actual = first.Equals(second);

            // assert
            Assert.Equal(true, actual);
        }
    }
}
