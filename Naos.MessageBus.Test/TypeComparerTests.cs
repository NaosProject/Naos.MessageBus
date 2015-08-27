// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeComparerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.HandlingContract;

    using Xunit;

    public class TypeComparerTests
    {
        [Fact]
        public void EqualsNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x, y);
            Assert.True(actual);
        }

        [Fact]
        public void EqualsAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x, y);
            Assert.True(actual);
        }

        [Fact]
        public void EqualsNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x, y);
            Assert.False(actual);
        }

        [Fact]
        public void EqualsAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x, y);
            Assert.False(actual);
        }
    }
}
