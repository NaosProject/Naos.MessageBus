// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendParcelMessageHandlerTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Instrumentation;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;

    using Xunit;

    public static class SendParcelMessageHandlerTest
    {
        [Fact]
        public static void HandleAsync__NullParcel__Throws()
        {
            // arrange
            var message = new SendParcelMessage();
            var handler = new SendParcelMessageHandler();
            Action testCode = () => handler.HandleAsync(message).Wait();

            // act & assert
            testCode.ShouldThrow<AggregateException>().WithInnerException<ArgumentException>().WithInnerMessage("No parcel provided to send.");
        }

        [Fact]
        public static void HandleAsync__ValidParcelProvided__ParcelIsSent()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes = new[] { new WaitMessage().ToAddressedMessage(new SimpleChannel("channel")).ToEnvelope(envelopeMachine) }
                             };

            var message = new SendParcelMessage { ParcelToSend = parcel };

            var trackingCalls = new List<string>();
            var trackingParcelsFromSent = new List<Parcel>();
            var postOffice = new PostOffice(
                                 Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingParcelsFromSent)(),
                                 new ChannelRouter(new NullChannel()),
                                 envelopeMachine);
            var handler = new SendParcelMessageHandler();

            // act
            handler.HandleAsync(message, postOffice).Wait();

            // assert
            trackingParcelsFromSent.Single().Id.Should().Be(parcel.Id);
        }

        [Fact]
        public static void HandleAsync__ValidParcelProvided__TrackingCodeIsShared()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes = new[] { new WaitMessage().ToAddressedMessage(new SimpleChannel("channel")).ToEnvelope(envelopeMachine) }
                             };

            var message = new SendParcelMessage { ParcelToSend = parcel };

            var trackingCalls = new List<string>();
            var trackingParcelsFromSent = new List<Parcel>();
            var postOffice = new PostOffice(
                                 Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingParcelsFromSent)(),
                                 new ChannelRouter(new NullChannel()),
                                 envelopeMachine);
            var handler = new SendParcelMessageHandler { TrackingCodes = null };

            // act
            handler.HandleAsync(message, postOffice).Wait();

            // assert
            handler.TrackingCodes.Single().ParcelId.Should().NotBe(default(Guid));
            handler.TrackingCodes.Single().EnvelopeId.Should().NotBe(default(Guid).ToString());
            Guid.Parse(handler.TrackingCodes.Single().EnvelopeId);
        }
    }
}
