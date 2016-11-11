// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeDescriptionTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.MessageBus.Domain;

    using OBeautifulCode.Reflection;

    using Xunit;

    public class TypeDescriptionTests
    {
        [Fact]
        public void FromType_ValidType_ValidTypeDescription()
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
        public void FromType_NullType_Throws()
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
        public void ToTypeDescription_ValidType_ValidTypeDescription()
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
