// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DispatcherFactoryDebugTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    public class DispatcherFactoryDebugTests
    {
        [Fact(Skip = "Debug test designed to run while connected through VPN.")]
        public void Test___Reflection___Load()
        {
            var assemblyPath = @"D:\Temp\other\";
            var dispatcherFactory = new DispatcherFactory(
                                        assemblyPath,
                                        new[] { new SimpleChannel("simple") },
                                        TypeMatchStrategy.NamespaceAndName,
                                        TimeSpan.FromSeconds(1),
                                        new NullParcelTrackingSystem(),
                                        new InMemoryActiveMessageTracker(),
                                        new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new SimpleChannel("default"))));
        }
    }
}
