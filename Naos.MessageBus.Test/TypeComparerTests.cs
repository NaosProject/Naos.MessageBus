// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeComparerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
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

        [Fact]
        public void EqualsStringsNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.True(actual);
        }

        [Fact]
        public void EqualsStringsAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.True(actual);
        }

        [Fact]
        public void EqualsStringsNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.False(actual);
        }

        [Fact]
        public void EqualsStringsAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.False(actual);
        }

        [Fact]
        public void EqualsDescriptionNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.True(actual);
        }

        [Fact]
        public void EqualsDescriptionAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.True(actual);
        }

        [Fact]
        public void EqualsDescriptionNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.False(actual);
        }

        [Fact]
        public void EqualsDescriptionAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.False(actual);
        }
    }
}
