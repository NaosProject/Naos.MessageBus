// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeComparerTests.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public static class TypeComparerTests
    {
        [Fact]
        public static void Equals_Nulls_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);

            Assert.False(comparer.Equals((Type)null, (Type)null));
            Assert.False(comparer.Equals((TypeDescription)null, (TypeDescription)null));

            Assert.False(comparer.Equals(typeof(string), (Type)null));
            Assert.False(comparer.Equals((Type)null, typeof(string)));

            Assert.False(comparer.Equals(typeof(string).ToTypeDescription(), (TypeDescription)null));
            Assert.False(comparer.Equals((TypeDescription)null, typeof(string).ToTypeDescription()));
        }

        [Fact]
        public static void EqualsNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x, y);
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x, y);
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x, y);
            Assert.False(actual);
        }

        [Fact]
        public static void EqualsAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x, y);
            Assert.False(actual);
        }

        [Fact]
        public static void EqualsStringsNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsStringsAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsStringsNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.False(actual);
        }

        [Fact]
        public static void EqualsStringsAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.Namespace, x.Name, x.AssemblyQualifiedName, y.Namespace, y.Name, y.AssemblyQualifiedName);
            Assert.False(actual);
        }

        [Fact]
        public static void EqualsDescriptionNamespaceAndName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsDescriptionAssemblyQualifiedName_Match_True()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(string);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.True(actual);
        }

        [Fact]
        public static void EqualsDescriptionNamespaceAndName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.False(actual);
        }

        [Fact]
        public static void EqualsDescriptionAssemblyQualifiedName_NoMatch_False()
        {
            var comparer = new TypeComparer(TypeMatchStrategy.AssemblyQualifiedName);
            var x = typeof(string);
            var y = typeof(Type);
            var actual = comparer.Equals(x.ToTypeDescription(), y.ToTypeDescription());
            Assert.False(actual);
        }

        [Fact]
        public static void ListTypeIsTypeDescriptedAndDeserializedWithGenericInfo()
        {
            var obj = new List<string>(new[] { "hello" }).ToArray();
            var json = obj.ToJson();
            var sharedProperty = new SharedProperty
                                     {
                                         Name = "Property",
                                         ValueAsJson = json,
                                         ValueType = obj.GetType().ToTypeDescription()
                                     };

            var fromSharedPropertyRaw = SharedPropertyHelper.GetValueFromPropertyEntry(
                TypeMatchStrategy.NamespaceAndName,
                sharedProperty);

            Assert.NotNull(fromSharedPropertyRaw);
            Assert.IsType(obj.GetType(), fromSharedPropertyRaw);
            var fromSharePropertyTyped = (string[])fromSharedPropertyRaw;
            Assert.Equal(obj.Single(), fromSharePropertyTyped.Single());
        }

        [Fact]
        public static void ResolveArraysOfKnownResolveTypeDescriptionFromAllLoadedTypes_ArrayOfType_ResolvesIfTypeKnown()
        {
            var obj = new[] { "hello" };
            var typeDescription = obj.GetType().ToTypeDescription();
            var type = SharedPropertyHelper.ResolveTypeDescriptionFromAllLoadedTypes(
                TypeMatchStrategy.NamespaceAndName,
                typeDescription);
            Assert.NotNull(type);
            Assert.Equal(obj.GetType(), type);
        }

        [Fact]
        public static void ResolveTypeDescriptionFromAllLoadedTypes_NotFound_ReturnsNull()
        {
            var description = new TypeDescription { AssemblyQualifiedName = "Monkeys", Name = "Are", Namespace = "Cool" };
            var ret = SharedPropertyHelper.ResolveTypeDescriptionFromAllLoadedTypes(TypeMatchStrategy.AssemblyQualifiedName, description);
            Assert.Null(ret);
        }
    }
}
