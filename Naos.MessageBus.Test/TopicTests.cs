// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicTests.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using Naos.MessageBus.Domain;

    using Xunit;

    public static class TopicTests
    {
        [Fact]
        public static void Impacting_Equal_AreEqual()
        {
            var firstName = "name1";

            var first = new AffectedTopic(firstName);

            var secondName = firstName;

            var second = new AffectedTopic(secondName);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Impacting_NotEqualAreNotEqual_Name()
        {
            var firstName = "name1";

            var first = new AffectedTopic(firstName);

            var secondName = "name2";

            var second = new AffectedTopic(secondName);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Dependent_Equal_AreEqual()
        {
            var firstName = "name1";

            var first = new DependencyTopic(firstName);

            var secondName = firstName;

            var second = new DependencyTopic(secondName);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Dependent_NotEqualAreNotEqual_Name()
        {
            var firstName = "name1";

            var first = new DependencyTopic(firstName);

            var secondName = "name2";

            var second = new DependencyTopic(secondName);

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Dependent_And_Impacting_EqualAreEqual_Type()
        {
            var firstName = "name1";

            var first = new AffectedTopic(firstName);

            var secondName = firstName;

            var second = new DependencyTopic(secondName);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Dependent_And_Named_EqualAreEqual_Type()
        {
            var firstName = "name1";

            var first = new NamedTopic(firstName);

            var secondName = firstName;

            var second = new DependencyTopic(secondName);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void Named_And_Impacting_EqualAreEqual_Type()
        {
            var firstName = "name1";

            var first = new AffectedTopic(firstName);

            var secondName = firstName;

            var second = new NamedTopic(secondName);

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }
    }
}
