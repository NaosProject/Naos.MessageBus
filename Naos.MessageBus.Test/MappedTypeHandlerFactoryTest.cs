// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappedTypeHandlerFactoryTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public static class MappedTypeHandlerFactoryTest
    {
        [Fact]
        public static void BuildHandlerForMessageType___SeededType___Builds()
        {
            // Arrange
            var messageType = typeof(FetchAndShareLatestTopicStatusReportsMessage);
            var handlerType = typeof(FetchAndShareLatestTopicStatusReportsMessageHandler);
            var handlerFactory = new MappedTypeHandlerFactory(new Dictionary<Type, Type> { { messageType, handlerType } }, TypeMatchStrategy.NamespaceAndName);

            // Act
            var actual = handlerFactory.BuildHandlerForMessageType(messageType);

            // Assert
            actual.Should().NotBeNull();
            actual.GetType().Should().Be(handlerType);
        }
    }
}
