// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
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

    public static class DispatcherFactoryTest
    {
        [Fact(Skip = "Used for debugging specific assembly sets that have issues with reflection loading.")]
        public static void IsolateReflectionIssue()
        {
            // arrange
            var directory = @"D:\Temp\FailedToReflect";
            var dispatcherFactory = new DispatcherFactory(
                directory,
                new List<IChannel>(),
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(30),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new NullChannel())));

            // act
            var dispatcher = dispatcherFactory.Create();

            // assert
            dispatcher.Should().NotBeNull();
        }
    }
}
