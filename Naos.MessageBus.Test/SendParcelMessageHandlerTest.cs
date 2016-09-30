// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SendParcelMessageHandlerTest.cs" company="Naos">
//   Copyright 2015 Naos
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

    public class SendParcelMessageHandlerTest
    {
        [Fact]
        public void HandleAsync__NullParcel__Throws()
        {
            // arrange
            var message = new SendParcelMessage();
            var handler = new SendParcelMessageHandler();
            Action testCode = () => handler.HandleAsync(message).Wait();

            // act & assert
            testCode.ShouldThrow<AggregateException>().WithInnerException<ArgumentException>().WithInnerMessage("No parcel provided to send.");
        }

        [Fact]
        public void HandleAsync__ValidParcelProvided__ParcelIsSent()
        {
            // arrange
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes = new[] { new WaitMessage().ToAddressedMessage(new SimpleChannel("channel")).ToEnvelope() }
                             };

            var message = new SendParcelMessage { ParcelToSend = parcel };

            var trackingCalls = new List<string>();
            var trackingParcelsFromSent = new List<Parcel>();
            var postOffice = new PostOffice(
                                 Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingParcelsFromSent)(),
                                 new ChannelRouter(new NullChannel()));
            var handler = new SendParcelMessageHandler();

            // act
            handler.HandleAsync(message, postOffice).Wait();

            // assert
            trackingParcelsFromSent.Single().Id.Should().Be(parcel.Id);
        }

        [Fact]
        public void HandleAsync__ValidParcelProvided__TrackingCodeIsShared()
        {
            // arrange
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes = new[] { new WaitMessage().ToAddressedMessage(new SimpleChannel("channel")).ToEnvelope() }
                             };

            var message = new SendParcelMessage { ParcelToSend = parcel };

            var trackingCalls = new List<string>();
            var trackingParcelsFromSent = new List<Parcel>();
            var postOffice = new PostOffice(
                                 Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingParcelsFromSent)(),
                                 new ChannelRouter(new NullChannel()));
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
