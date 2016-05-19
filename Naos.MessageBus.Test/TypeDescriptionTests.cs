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

    using Xunit;

    public class TypeDescriptionTests
    {
        [Fact]
        public void FromType_ValidType_ValidTypeDescription()
        {
            // arrange
            var type = typeof(string);

            // act
            var description = TypeDescription.FromType(type);

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
                    TypeDescription.FromType(null);
                };

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Type cannot be null");
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

        [Fact]
        public void ToTypeDescription_NullType_Throws()
        {
            // arrange
            Action testCode = () =>
                {
                    Type type = null;
                    type.ToTypeDescription();
                };

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Type cannot be null");
        }
    }
}
