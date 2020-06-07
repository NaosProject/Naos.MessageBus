// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeRepresentationTests.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.MessageBus.Domain;
    using OBeautifulCode.Representation.System;
    using OBeautifulCode.Type;

    using Xunit;

    public static class TypeRepresentationTests
    {
        [Fact]
        public static void FromType_ValidType_ValidTypeRepresentation()
        {
            // arrange
            var type = typeof(string);

            // act
            var description = type.ToRepresentation();

            // assert
            type.AssemblyQualifiedName.Should().Contain(description.BuildAssemblyQualifiedName());
            Assert.Equal(type.Namespace, description.Namespace);
            Assert.Equal(type.Name, description.Name);
        }

        [Fact]
        public static void FromType_NullType_Throws()
        {
            // arrange
            Action testCode = () =>
                {
                    ((Type)null).ToRepresentation();
                };

            // act & assert
            testCode.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public static void ToRepresentation_ValidType_ValidTypeRepresentation()
        {
            // arrange
            var type = typeof(string);

            // act
            var description = type.ToRepresentation();

            // assert
            type.AssemblyQualifiedName.Should().Contain(description.BuildAssemblyQualifiedName());
            Assert.Equal(type.Namespace, description.Namespace);
            Assert.Equal(type.Name, description.Name);
        }
    }
}
