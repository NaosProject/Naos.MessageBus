// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcherTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.Hangfire.Harness;

    using SimpleInjector;

    using Xunit;

    public class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container()).Dispatch(null);
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel cannot be null", ex.Message);
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container()).Dispatch(new Parcel());
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container()).Dispatch(new Parcel { Envelopes = new List<Envelope>() });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }
    }
}
