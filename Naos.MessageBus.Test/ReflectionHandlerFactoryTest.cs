// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionHandlerFactoryTest.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.Type;

    using Xunit;

    public static class ReflectionHandlerFactoryTest
    {
        [Fact]
        public static void BuildHandlerForMessageType___LoadedType___Builds()
        {
            // Arrange
            var messageType = typeof(FetchAndShareLatestTopicStatusReportsMessage);
            var handlerType = typeof(FetchAndShareLatestTopicStatusReportsMessageHandler);
            var handlerFactory = new ReflectionHandlerFactory(TypeMatchStrategy.NamespaceAndName);

            // Act
            var actual = handlerFactory.BuildHandlerForMessageType(messageType);

            // Assert
            actual.Should().NotBeNull();
            actual.GetType().Should().Be(handlerType);
        }

        [Fact(Skip = "Not sure if this makes sense to support")]
        public static void BuildHandlerForMessageType___LoadedTypeMultipleLayersOfInheritanceDeep___Builds()
        {
            /*

           public class NestedMessage : IMessage
           {
               public string Description { get; set; }
           }

           public abstract class NestedBase : MessageHandlerBase<NestedMessage>
           {
           }

           public class NestedMessageHandler : NestedBase
           {
               public override Task HandleAsync(NestedMessage message)
               {
                   return Task.Run(
                       () =>
                           {
                           });
                }
           }

           // Arrange
           var messageType = typeof(NestedMessage);
           var handlerType = typeof(NestedMessageHandler);
           var handlerFactory = new ReflectionHandlerFactory(TypeMatchStrategy.NamespaceAndName);

           // Act
           var actual = handlerFactory.BuildHandlerForMessageType(messageType);

           // Assert
           actual.Should().NotBeNull();
           actual.GetType().Should().Be(handlerType);
           */
        }
    }
}