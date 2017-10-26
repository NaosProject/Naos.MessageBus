// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcherTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Naos.Compression.Domain;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.Serialization.Domain;
    using Naos.Serialization.Factory;
    using Naos.Serialization.Factory.Extensions;
    using Naos.Serialization.Json;

    using OBeautifulCode.TypeRepresentation;

    using Xunit;

    using static System.FormattableString;

    public static class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_ParcelWithSharesThatMatchEnum_FullTrip()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var channel = new SimpleChannel("el-channel");

            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>
                                                              {
                                                                      { typeof(FirstEnumMessage), typeof(FirstEnumHandler) },
                                                                      { typeof(SecondEnumMessage), typeof(SecondEnumHandler) }
                                                              };

            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);

            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var firstMessage = new FirstEnumMessage() { Description = "RunMe 1", SeedValue = MyEnum.OtherValue };
            var secondMessage = new SecondEnumMessage() { Description = "RunMe 2" };
            var thirdMessage = new SecondEnumMessage() { Description = "RunMe 3" };

            var messageSequence = new MessageSequence
                                      {
                                          AddressedMessages =
                                              new[]
                                                  {
                                                      new AddressedMessage
                                                          {
                                                              Address = channel,
                                                              Message = firstMessage
                                                          },
                                                      new AddressedMessage
                                                          {
                                                              Address = channel,
                                                              Message = secondMessage
                                                          },
                                                      new AddressedMessage
                                                          {
                                                              Address = channel,
                                                              Message = thirdMessage
                                                          }
                                                  }
                                      };

            var envelopesFromSequence = messageSequence.AddressedMessages.Select(addressedMessage => addressedMessage.ToEnvelope(envelopeMachine)).ToList();

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            var firstTrackingCode = new TrackingCode { EnvelopeId = "1" };
            dispatcher.Dispatch("First Message", firstTrackingCode, parcel, channel);

            // verify remaining envelope got sent
            Assert.Equal(1, trackingSends.Count);

            var newParcel = trackingSends.Single();
            Assert.Equal(1, newParcel.SharedInterfaceStates.Count);

            var sharedPropertySet = newParcel.SharedInterfaceStates.Single();
            var typeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            Assert.True(
                typeComparer.Equals(typeof(IShareEnum).ToTypeDescription(), sharedPropertySet.InterfaceType));
            Assert.Equal("EnumValueToShare", sharedPropertySet.Properties.Single().Name);
            var jsoner = new NaosJsonSerializer();

            var seedValueAsJson = jsoner.SerializeToString(firstMessage.SeedValue);
            Assert.Equal(seedValueAsJson, sharedPropertySet.Properties.Single().SerializedValue.SerializedPayload);

            var secondTrackingCode = new TrackingCode { EnvelopeId = "2" };
            dispatcher.Dispatch("Second Message", secondTrackingCode, newParcel, channel);

            // verify new message
            Assert.Equal(2, trackingSends.Count);
            var newNewParcel = trackingSends.Single(_ => _.Envelopes.First().Description == thirdMessage.Description);
            Assert.Equal(2, newNewParcel.SharedInterfaceStates.Count);
            Assert.Equal(typeof(FirstEnumHandler).ToTypeDescription().Name, newNewParcel.SharedInterfaceStates.First().SourceType.Name);
            Assert.Equal(typeof(SecondEnumHandler).ToTypeDescription().Name, newNewParcel.SharedInterfaceStates.Skip(1).First().SourceType.Name);
        }

        [Fact]
        public static void Dispatch_ParcelWithRemainingEnvelopes_RemainingEnvelopesDoNotGetDeserialized()
        {
            // arrange
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var channel = new SimpleChannel("el-channel");

            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>
                                                              {
                                                                      { typeof(FirstEnumMessage), typeof(FirstEnumHandler) },
                                                                      { typeof(SecondEnumMessage), typeof(SecondEnumHandler) }
                                                              };

            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, handlerInterfaceToImplementationTypeMap, channel);

            var firstMessage = new FirstEnumMessage() { Description = "RunMe 1", SeedValue = MyEnum.OtherValue };

            var envelopesFromSequence = new[]
                                            {
                                                firstMessage.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                new Envelope(
                                                    "2",
                                                    "No work",
                                                    channel,
                                                    new DescribedSerialization(
                                                        new TypeDescription("Not Real Space", "Not Real Name", "Not Real AQN"),
                                                        "Not Real Payload",
                                                        PostOffice.MessageSerializationDescription))
                                            };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch("First Message", new TrackingCode(), parcel, channel);

            // assert

            // by virtue of not throwing we succeeded because the second message in the sequence won't deserialize...
        }

        [Fact]
        public static void Dispatch_ParcelWithNonSharedMessages_Succeeds()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();

            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            // arrange
            var channel = new SimpleChannel("el-channel");

            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>
                                                              {
                                                                      { typeof(MessageOne), typeof(MessageOneHandler) },
                                                                      { typeof(MessageTwo), typeof(MessageTwoHandler) }
                                                              };

            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, handlerInterfaceToImplementationTypeMap, channel);

            var firstMessage = new MessageOne() { Description = "RunMe 1" };
            var secondMessage = new MessageTwo() { Description = "RunMe 2" };

            var envelopesFromSequence = new[]
                                            {
                                                firstMessage.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                secondMessage.ToAddressedMessage(channel).ToEnvelope(envelopeMachine)
                                            };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch("First Message", new TrackingCode(), parcel, channel);
            Assert.Equal(1, trackingSends.Count);
            var nextMessage = trackingSends.Single();
            trackingSends.Clear();
            dispatcher.Dispatch("Second Message", new TrackingCode(), nextMessage, channel);
            Assert.Equal(0, trackingSends.Count);

            // assert

            // by virtue of not throwing we succeeded because the messages didn't throw...
        }

        [Fact]
        public static void Dispatch_ParcelWithShareableMessagesAndNoShares_Succeeds()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();

            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            // arrange
            var channel = new SimpleChannel("el-channel");

            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>
                                                              {
                                                                      { typeof(MessageOneShare), typeof(MessageOneShareHandler) },
                                                                      { typeof(MessageTwoShare), typeof(MessageTwoShareHandler) }
                                                              };

            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, handlerInterfaceToImplementationTypeMap, channel);

            var firstMessage = new MessageOneShare() { Description = "RunMe 1" };
            var secondMessage = new MessageTwoShare() { Description = "RunMe 2" };

            var envelopesFromSequence = new[]
                                            {
                                                firstMessage.ToAddressedMessage(channel).ToEnvelope(envelopeMachine),
                                                secondMessage.ToAddressedMessage(channel).ToEnvelope(envelopeMachine)
                                            };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch("First Message", new TrackingCode(), parcel, channel);
            Assert.Equal(1, trackingSends.Count);
            var nextMessage = trackingSends.Single();
            trackingSends.Clear();
            dispatcher.Dispatch("Second Message", new TrackingCode(), nextMessage, channel);
            Assert.Equal(0, trackingSends.Count);

            // assert

            // by virtue of not throwing we succeeded because the messages didn't throw...
        }

        private static MessageDispatcher GetMessageDispatcher(List<Parcel> trackingSends, IReadOnlyDictionary<Type, Type> container, IChannel channel)
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var activeMessageTracker = new InMemoryActiveMessageTracker();

            var handlerBuilder = new MappedTypeHandlerFactory(container, TypeMatchStrategy.NamespaceAndName);
            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                activeMessageTracker,
                senderConstructor(),
                envelopeMachine,
                shareManager);
            return dispatcher;
        }

        [Fact]
        public static void Dispatch_IncrementsAndDecrementsTracker()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var activeMessageTracker = new InMemoryActiveMessageTracker();
            var channel = new SimpleChannel("el-channel");
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(WaitMessage), typeof(WaitMessageHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);
            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                activeMessageTracker,
                new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new NullChannel()), envelopeMachine),
                envelopeMachine,
                shareManager);

            var message = new WaitMessage { Description = "RunMe", TimeToWait = TimeSpan.FromSeconds(3) };
            var envelope = message.ToAddressedMessage(channel).ToEnvelope(envelopeMachine);

            Assert.Equal(0, activeMessageTracker.ActiveMessagesCount);
            ThreadPool.QueueUserWorkItem(state => dispatcher.Dispatch("RunMe", new TrackingCode(), new Parcel { Envelopes = new[] { envelope } }, channel));
            Thread.Sleep(2000);
            Assert.Equal(1, activeMessageTracker.ActiveMessagesCount);
            Thread.Sleep(4000);
            Assert.Equal(0, activeMessageTracker.ActiveMessagesCount);
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelNamespaceNameMatch_Resends()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                new NullHandlerBuilder(),
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var validParcel = new Parcel { Envelopes = new[] { new NullMessage().ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine), } };

            var notMonitoredChannel = new SimpleChannel("OtherChannel");
            var invalidParcel = new Parcel { Envelopes = new[] { new NullMessage().ToAddressedMessage(notMonitoredChannel).ToEnvelope(envelopeMachine) } };

            dispatcher.Dispatch("ValidParcel", new TrackingCode(), validParcel, monitoredChannel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch("InvalidParcel", new TrackingCode(), invalidParcel, notMonitoredChannel);
            Assert.Equal(1, trackingSends.Count);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AndRe", Justification = "Spelling/name is correct.")]
        [Fact]
        public static void Dispatch_DispatchingMethodWithAbortAndResend_TracksAddressedThenAbortAndResends()
        {
            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(ThrowsExceptionMessage), typeof(ThrowsExceptionMessageHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);

            var trackingCalls = new List<string>();
            var trackingSends = new List<Parcel>();
            var trackingSendsFromTracker = new List<Parcel>();
            var trackingConstructor = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSendsFromTracker);
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                trackingConstructor(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var exception = new AbortParcelDeliveryException("Abort") { Reschedule = true };
            var parcel = new Parcel
                             {
                                 Envelopes = new[]
                                                 {
                                                     new ThrowsExceptionMessage()
                                                         {
                                                             SerializedExceptionToThrow = exception.ToDescribedSerialization(
                                                                 PostOffice.MessageSerializationDescription)
                                                         }.ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine)
                                                 }
                             };

            dispatcher.Dispatch("Parcel", new TrackingCode(), parcel, monitoredChannel);
            trackingSends.Should().HaveCount(1);
            trackingCalls.Should().BeEquivalentTo(nameof(IParcelTrackingSystem.UpdateAttemptingAsync), nameof(IParcelTrackingSystem.UpdateAbortedAsync));
        }

        [Fact]
        public static void Dispatch_DispatchingMethodWithAbortAndNoResend_TracksAddressedThenAbortAndDoesNotSend()
        {
            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            HandlerToolshed.InitializeSerializerFactory(() => SerializerFactory.Instance);
            HandlerToolshed.InitializeCompressorFactory(() => CompressorFactory.Instance);
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(ThrowsExceptionMessage), typeof(ThrowsExceptionMessageHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);

            var trackingCalls = new List<string>();
            var trackingSendsFromTracker = new List<Parcel>();
            var trackingConstructor = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSendsFromTracker);

            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                trackingConstructor(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var exception = new AbortParcelDeliveryException("Abort") { Reschedule = false };
            var parcel = new Parcel
            {
                Envelopes =
                                     new[]
                                         {
                                             new ThrowsExceptionMessage()
                                                 {
                                                     SerializedExceptionToThrow = exception.ToDescribedSerialization(PostOffice.MessageSerializationDescription)
                                                 }.ToAddressedMessage(
                                                     monitoredChannel).ToEnvelope(envelopeMachine)
                                         }
            };

            dispatcher.Dispatch("Parcel", new TrackingCode(), parcel, monitoredChannel);
            trackingSends.Should().HaveCount(0);
            trackingCalls.Should().BeEquivalentTo(nameof(IParcelTrackingSystem.UpdateAttemptingAsync), nameof(IParcelTrackingSystem.UpdateAbortedAsync));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Keeping this way for now.")]
        [Fact]
        public static void Dispatch_DispatchingMethodWithException_TracksAddressedThenRejectedAndThrows()
        {
            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            HandlerToolshed.InitializeSerializerFactory(() => SerializerFactory.Instance);
            HandlerToolshed.InitializeCompressorFactory(() => CompressorFactory.Instance);
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(ThrowsExceptionMessage), typeof(ThrowsExceptionMessageHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);

            var trackingCalls = new List<string>();
            var trackingSendsFromTracker = new List<Parcel>();
            var trackingConstructor = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSendsFromTracker);

            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                trackingConstructor(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var exception = new NullReferenceException("Failed");
            var parcel = new Parcel
                             {
                                 Envelopes = new[]
                                                 {
                                                     new ThrowsExceptionMessage()
                                                         {
                                                             SerializedExceptionToThrow = exception.ToDescribedSerialization(
                                                                 PostOffice.MessageSerializationDescription)
                                                         }.ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine)
                                                 }
                             };

            Action testCode = () => dispatcher.Dispatch("Parcel", new TrackingCode(), parcel, monitoredChannel);
            testCode.ShouldThrow<NullReferenceException>().WithMessage(exception.Message);

            trackingSends.Should().HaveCount(0);
            trackingCalls.Should().BeEquivalentTo(nameof(IParcelTrackingSystem.UpdateAttemptingAsync), nameof(IParcelTrackingSystem.UpdateRejectedAsync));
        }

        [Fact]
        public static void Dispatch_DispatchingMethodWithSuccess_TracksAddressedThenDelivered()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var trackingCalls = new List<string>();
            var trackingSendsFromTracker = new List<Parcel>();
            var trackingConstructor = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSendsFromTracker);

            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                new NullHandlerBuilder(),
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                trackingConstructor(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var parcel = new Parcel { Envelopes = new[] { new NullMessage().ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine) } };

            dispatcher.Dispatch("Parcel", new TrackingCode(), parcel, monitoredChannel);

            trackingSends.Should().HaveCount(0);
            trackingCalls.Should().BeEquivalentTo(nameof(IParcelTrackingSystem.UpdateAttemptingAsync), nameof(IParcelTrackingSystem.UpdateDeliveredAsync));
        }

        [Fact]
        public static void Dispatch_DispatchingMethodWithRecurringHeaderMessage_ResendsWithoutTracking()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var trackingCalls = new List<string>();
            var trackingSendsFromTracker = new List<Parcel>();
            var trackingConstructor = Factory.GetInMemoryParcelTrackingSystem(trackingCalls, trackingSendsFromTracker);

            var trackingSends = new List<Parcel>();
            var senderConstructor = Factory.GetInMemorySender(trackingSends);

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                new NullHandlerBuilder(),
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                trackingConstructor(),
                new InMemoryActiveMessageTracker(),
                senderConstructor(),
                envelopeMachine,
                shareManager);

            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new[]
                                         {
                                             new RecurringHeaderMessage().ToAddressedMessage().ToEnvelope(envelopeMachine),
                                             new NullMessage().ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine),
                                         }
                             };

            dispatcher.Dispatch("Parcel", new TrackingCode(), parcel, monitoredChannel);
            trackingSends.Should().HaveCount(1);
            trackingCalls.Should().HaveCount(0);
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelAssemblyQualifiedMatch_Resends()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var trackingSends = new List<Parcel>();

            var monitoredChannel = new SimpleChannel("ChannelName");
            var dispatcher = new MessageDispatcher(
                new NullHandlerBuilder(),
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                Factory.GetInMemorySender(trackingSends)(),
                envelopeMachine,
                shareManager);

            var validParcel = new Parcel { Envelopes = new[] { new NullMessage().ToAddressedMessage(monitoredChannel).ToEnvelope(envelopeMachine) } };

            var notMonitoredChannel = new SimpleChannel("OtherChannel");
            var invalidParcel = new Parcel { Envelopes = new[] { new NullMessage().ToAddressedMessage(notMonitoredChannel).ToEnvelope(envelopeMachine) } };

            dispatcher.Dispatch("ValidParcel", new TrackingCode(), validParcel, monitoredChannel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch("InvalidParcel", new TrackingCode(), invalidParcel, notMonitoredChannel);
            Assert.Equal(1, trackingSends.Count);
        }

        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () =>
            GetMessageDispatcher().Dispatch("Name", new TrackingCode(), null, new NullChannel());
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel cannot be null");
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => GetMessageDispatcher().Dispatch("Name", new TrackingCode(), new Parcel(), new NullChannel());
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel must contain envelopes");
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => GetMessageDispatcher().Dispatch("Name", new TrackingCode(), new Parcel { Envelopes = new List<Envelope>() }, new NullChannel());
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel must contain envelopes");
        }

        [Fact]
        public static void Dispatch_EnvelopeWithUnregisteredType_Throws()
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            Action testCode = () =>
                {
                    var channel = new SimpleChannel("Channel");

                    var message = new NullMessage();
                    GetMessageDispatcher(new[] { channel }).Dispatch(
                        "Name",
                        new TrackingCode(),
                        new Parcel
                            {
                                Id = Guid.Empty,
                                Envelopes = new List<Envelope>(new[] { message.ToAddressedMessage(channel).ToEnvelope(envelopeMachine) })
                            },
                        channel);
                };

            testCode.ShouldThrow<FailedToFindHandlerException>().Where(_ => _.Message.StartsWith("Could not find a handler for the specified type; Parcel: Parcel ID: 00000000-0000-0000-0000-000000000000, Envelope ID: [null], Specified Message Type: Naos.MessageBus.Domain.NullMessage"));
        }

        [Fact]
        public static void Dispatch_InitialStateRequirement_GetsGenerated()
        {
            StateHandler.StateHistory.Clear();
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(InitialStateMessage), typeof(StateHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);
            var message = new InitialStateMessage();

            var channel = new SimpleChannel("fakeChannel");
            var messageDispatcher = new MessageDispatcher(handlerBuilder, new ConcurrentDictionary<Type, object>(), new[] { channel }, new HarnessStaticDetails(), new NullParcelTrackingSystem(), new InMemoryActiveMessageTracker(), new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new NullChannel()), envelopeMachine), envelopeMachine, shareManager);
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                     new[] { message.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id") })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", new TrackingCode(), parcel, channel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CallUses", Justification = "Spelling/name is correct.")]
        [Fact]
        public static void Dispatch_InitialStateRequirementRunTwice_SecondCallUsesPreviousState()
        {
            StateHandler.StateHistory.Clear();
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(InitialStateMessage), typeof(StateHandler) } };
            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);
            var message = new InitialStateMessage();

            var channel = new SimpleChannel("fakeChannel");
            var messageDispatcher = new MessageDispatcher(handlerBuilder, new ConcurrentDictionary<Type, object>(), new[] { channel }, new HarnessStaticDetails(), new NullParcelTrackingSystem(), new InMemoryActiveMessageTracker(), new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new NullChannel()), envelopeMachine), envelopeMachine, shareManager);
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                         new[] { message.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id") })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", new TrackingCode(), parcel, channel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = true; // this will say that the state is valid and should NOT re-generate
            messageDispatcher.Dispatch("Parcel Name", new TrackingCode(), parcel, channel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["ValidateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);
            Assert.False(StateHandler.StateHistory.ContainsKey("GenerateInitialState"));
        }

        [Fact]
        public static void Dispatch_InitialStateRequirementRunTwice_InvalidSecondCallGeneratesNewState()
        {
            StateHandler.StateHistory.Clear();
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type> { { typeof(InitialStateMessage), typeof(StateHandler) } };
            var message = new InitialStateMessage();

            var channel = new SimpleChannel("fakeChannel");
            var messageDispatcher = GetMessageDispatcher(new[] { channel }, handlerInterfaceToImplementationTypeMap);

            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                         new[] { message.ToAddressedMessage(channel).ToEnvelope(envelopeMachine, "id") })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", new TrackingCode(), parcel, channel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = false; // this will say that the state is NOT valid and should re-generate
            messageDispatcher.Dispatch("Parcel Name", new TrackingCode(), parcel, channel);
            Assert.Equal(3, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["SeedInitialState"],
                StateHandler.StateHistory["GenerateInitialState"]);
            Assert.NotEqual(
                StateHandler.StateHistory["ValidateInitialState"],
                StateHandler.StateHistory["GenerateInitialState"]);
            Assert.NotEqual(
                StateHandler.StateHistory["ValidateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);
        }

        private static MessageDispatcher GetMessageDispatcher(IList<IChannel> channels = null, Dictionary<Type, Type> handlerInterfaceToImplementationTypeMap = null)
        {
            var envelopeMachine = Factory.GetEnvelopeMachine();
            var shareManager = Factory.GetShareManager();

            if (channels == null)
            {
                channels = new List<IChannel>();
            }

            if (handlerInterfaceToImplementationTypeMap == null)
            {
                handlerInterfaceToImplementationTypeMap = new Dictionary<Type, Type>();
            }

            var handlerBuilder = new MappedTypeHandlerFactory(handlerInterfaceToImplementationTypeMap, TypeMatchStrategy.NamespaceAndName);

            return new MessageDispatcher(
                handlerBuilder,
                new ConcurrentDictionary<Type, object>(),
                channels,
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                new PostOffice(new NullParcelTrackingSystem(), new ChannelRouter(new NullChannel()), envelopeMachine),
                envelopeMachine,
                shareManager);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Keeping this way for now.")]
        public class StateHandler : MessageHandlerBase<InitialStateMessage>, INeedSharedState<string>
        {
            static StateHandler()
            {
                StateHistory = new Dictionary<string, string>();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Keeping this way for now.")]
            public static Dictionary<string, string> StateHistory { get; set; }

            public static bool ShouldValidate { get; set; }

            public override async Task HandleAsync(InitialStateMessage message)
            {
                /* no-op */
                await Task.FromResult<object>(null);
            }

            public string CreateState()
            {
                var state = Guid.NewGuid().ToString().ToUpperInvariant();
                StateHistory.Add("GenerateInitialState", state);
                return state;
            }

            public void PreHandleWithState(string sharedState)
            {
                StateHistory.Add("SeedInitialState", sharedState);
            }

            public bool IsStateStillValid(string sharedState)
            {
                StateHistory.Add("ValidateInitialState", sharedState);
                return ShouldValidate;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "Keeping this way for now.")]
        public class InitialStateMessage : IMessage
        {
            public string Description { get; set; }
        }
    }

    public class MessageOne : IMessage
    {
        public string Description { get; set; }
    }

    public class MessageOneHandler : MessageHandlerBase<MessageOne>
    {
        public override async Task HandleAsync(MessageOne message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public class MessageTwo : IMessage
    {
        public string Description { get; set; }
    }

    public class MessageTwoHandler : MessageHandlerBase<MessageTwo>
    {
        public override async Task HandleAsync(MessageTwo message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "Keeping for extension and reflection.")]
    public interface IShareNothing : IShare
    {
    }

    public class MessageOneShare : IMessage, IShareNothing
    {
        public string Description { get; set; }
    }

    public class MessageOneShareHandler : MessageHandlerBase<MessageOneShare>, IShareNothing
    {
        public override async Task HandleAsync(MessageOneShare message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public class MessageTwoShare : IMessage, IShareNothing
    {
        public string Description { get; set; }
    }

    public class MessageTwoShareHandler : MessageHandlerBase<MessageTwoShare>, IShareNothing
    {
        public override async Task HandleAsync(MessageTwoShare message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }
}
