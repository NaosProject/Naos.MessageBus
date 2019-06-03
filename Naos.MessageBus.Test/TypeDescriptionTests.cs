// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeDescriptionTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Type;

    using Xunit;

    public static class TypeDescriptionTests
    {
        [Fact]
        public static void FromType_ValidType_ValidTypeDescription()
        {
            // arrange
            var type = typeof(string);

            // act
            var description = type.ToTypeDescription();

            // assert
            Assert.Equal(type.AssemblyQualifiedName, description.AssemblyQualifiedName);
            Assert.Equal(type.Namespace, description.Namespace);
            Assert.Equal(type.Name, description.Name);
        }

        [Fact]
        public static void FromType_NullType_Throws()
        {
            // arrange
            Action testCode = () =>
                {
                    ((Type)null).ToTypeDescription();
                };

            // act & assert
            testCode.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public static void ToTypeDescription_ValidType_ValidTypeDescription()
        {
            // arrange
            var type = typeof(string);

            // act
            var description = type.ToTypeDescription();

            // assert
            Assert.Equal(type.AssemblyQualifiedName, description.AssemblyQualifiedName);
            Assert.Equal(type.Namespace, description.Namespace);
            Assert.Equal(type.Name, description.Name);
        }
    }
}
