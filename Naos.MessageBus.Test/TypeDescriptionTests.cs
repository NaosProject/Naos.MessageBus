// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeDescriptionTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

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
            Action testCode = () =>
                {
                    TypeDescription.FromType(null);
                };
            var ex = Assert.Throws<ArgumentException>(testCode);
            Assert.Equal("Type cannot be null", ex.Message);
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
            Action testCode = () =>
                {
                    Type type = null;
                    type.ToTypeDescription();
                };
            var ex = Assert.Throws<ArgumentException>(testCode);
            Assert.Equal("Type cannot be null", ex.Message);
        }
    }
}
