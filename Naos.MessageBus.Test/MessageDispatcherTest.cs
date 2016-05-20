// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcherTest.cs" company="Naos">
//   Copyright 2015 Naos
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

    using FakeItEasy;

    using FluentAssertions;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;

    using SimpleInjector;

    using Xunit;

    public class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_ParcelWithSharesThatMatchEnum_FullTrip()
        {
            // arrange
            var container = new Container();
            var trackingSends = new List<Parcel>();
            var senderConstructor = GetInMemorySender(trackingSends);

            var channel = new Channel("el-channel");

            container.Register<IHandleMessages<FirstEnumMessage>, FirstEnumHandler>();
            container.Register<IHandleMessages<SecondEnumMessage>, SecondEnumHandler>();

            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5),
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                senderConstructor());

            var firstMessage = new FirstEnumMessage() { Description = "RunMe 1", SeedValue = MyEnum.OtherValue };
            var secondMessage = new SecondEnumMessage() { Description = "RunMe 2" };
            var thirdMessage = new SecondEnumMessage() { Description = "RunMe 3" };
 
            var messageSequence = new MessageSequence
                                      {
                                          ChanneledMessages =
                                              new[]
                                                  {
                                                      new ChanneledMessage
                                                          {
                                                              Channel = channel,
                                                              Message = firstMessage
                                                          },
                                                      new ChanneledMessage
                                                          {
                                                              Channel = channel,
                                                              Message = secondMessage
                                                          },
                                                      new ChanneledMessage
                                                          {
                                                              Channel = channel,
                                                              Message = thirdMessage
                                                          }
                                                  }
                                      };

            var envelopesFromSequence = messageSequence.ChanneledMessages.Select(channeledMessage => channeledMessage.ToEnvelope()).ToList();

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            var firstTrackingCode = new TrackingCode { EnvelopeId = "1" };
            dispatcher.Dispatch(firstTrackingCode, "First Message", parcel);

            // verify remaining envelope got sent
            Assert.Equal(1, trackingSends.Count);

            var newParcel = trackingSends.Single();
            Assert.Equal(1, newParcel.SharedInterfaceStates.Count);

            var sharedPropertySet = newParcel.SharedInterfaceStates.Single();
            var typeComparer = new TypeComparer(TypeMatchStrategy.NamespaceAndName);
            Assert.True(
                typeComparer.Equals(typeof(IShareEnum).ToTypeDescription(), sharedPropertySet.InterfaceType)); 
            Assert.Equal("EnumValueToShare", sharedPropertySet.Properties.Single().Name);
            var seedValueAsJson = Serializer.Serialize(firstMessage.SeedValue);
            Assert.Equal(seedValueAsJson, sharedPropertySet.Properties.Single().ValueAsJson);

            var secondTrackingCode = new TrackingCode { EnvelopeId = "2" };
            dispatcher.Dispatch(secondTrackingCode, "Second Message", newParcel);

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
            var channel = new Channel("el-channel");
            var container = new Container();

            container.Register<IHandleMessages<FirstEnumMessage>, FirstEnumHandler>();
            container.Register<IHandleMessages<SecondEnumMessage>, SecondEnumHandler>();
            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, container, channel);

            var firstMessage = new FirstEnumMessage() { Description = "RunMe 1", SeedValue = MyEnum.OtherValue };

            var envelopesFromSequence = new[]
                                            {
                                                firstMessage.ToChanneledMessage(channel).ToEnvelope(),
                                                new Envelope(
                                                    "2",
                                                    "No work",
                                                    channel,
                                                    "WON'T WORK",
                                                    new TypeDescription
                                                        {
                                                            Namespace = "Namespace",
                                                            Name = "Name",
                                                            AssemblyQualifiedName = "AssemblyQualifiedName"
                                                        })
                                            };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch(new TrackingCode(), "First Message", parcel);

            // assert

            // by virtue of not throwing we succeeded because the second message in the sequence won't deserialize...
        }

        [Fact]
        public static void Dispatch_ParcelWithNonSharedMessages_Succeeds()
        {
            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            // arrange
            var channel = new Channel("el-channel");
            var container = new Container();
            container.Register<IHandleMessages<MessageOne>, MessageOneHandler>();
            container.Register<IHandleMessages<MessageTwo>, MessageTwoHandler>();

            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, container, channel);

            var firstMessage = new MessageOne() { Description = "RunMe 1" };
            var secondMessage = new MessageTwo() { Description = "RunMe 2" };

            var envelopesFromSequence = new[] { firstMessage.ToChanneledMessage(channel).ToEnvelope(), secondMessage.ToChanneledMessage(channel).ToEnvelope() };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch(new TrackingCode(), "First Message", parcel);
            Assert.Equal(1, trackingSends.Count);
            var nextMessage = trackingSends.Single();
            trackingSends.Clear();
            dispatcher.Dispatch(new TrackingCode(), "Second Message", nextMessage);
            Assert.Equal(0, trackingSends.Count);

            // assert

            // by virtue of not throwing we succeeded because the messages didn't throw...
        }

        [Fact]
        public static void Dispatch_ParcelWithShareableMessagesAndNoShares_Succeeds()
        {
            // skipping on appveyor because it hangs...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            // arrange
            var channel = new Channel("el-channel");
            var container = new Container();
            container.Register<IHandleMessages<MessageOneShare>, MessageOneShareHandler>();
            container.Register<IHandleMessages<MessageTwoShare>, MessageTwoShareHandler>();

            var trackingSends = new List<Parcel>();

            var dispatcher = GetMessageDispatcher(trackingSends, container, channel);

            var firstMessage = new MessageOneShare() { Description = "RunMe 1" };
            var secondMessage = new MessageTwoShare() { Description = "RunMe 2" };

            var envelopesFromSequence = new[] { firstMessage.ToChanneledMessage(channel).ToEnvelope(), secondMessage.ToChanneledMessage(channel).ToEnvelope() };

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            // act
            dispatcher.Dispatch(new TrackingCode(), "First Message", parcel);
            Assert.Equal(1, trackingSends.Count);
            var nextMessage = trackingSends.Single();
            trackingSends.Clear();
            dispatcher.Dispatch(new TrackingCode(), "Second Message", nextMessage);
            Assert.Equal(0, trackingSends.Count);

            // assert

            // by virtue of not throwing we succeeded because the messages didn't throw...
        }

        private static MessageDispatcher GetMessageDispatcher(List<Parcel> trackingSends, Container container, Channel channel)
        {
            var senderConstructor = GetInMemorySender(trackingSends);

            var activeMessageTracker = new InMemoryActiveMessageTracker();

            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.01),
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                activeMessageTracker,
                senderConstructor());
            return dispatcher;
        }

        [Fact]
        public static void Dispatch_IncrementsAndDecrementsTracker()
        {
            var activeMessageTracker = new InMemoryActiveMessageTracker();
            var channel = new Channel("el-channel");
            var container = new Container();
            container.Register<IHandleMessages<WaitMessage>, WaitMessageHandler>();
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5),
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                activeMessageTracker,
                new PostOffice(new NullCourier()));

            var message = new WaitMessage { Description = "RunMe", TimeToWait = TimeSpan.FromSeconds(3) };
            var envelope = message.ToChanneledMessage(channel).ToEnvelope();

            Assert.Equal(0, activeMessageTracker.ActiveMessagesCount);
            ThreadPool.QueueUserWorkItem(
                state => dispatcher.Dispatch(new TrackingCode(), "RunMe", new Parcel { Envelopes = new[] { envelope } }));
            Thread.Sleep(2000);
            Assert.Equal(1, activeMessageTracker.ActiveMessagesCount);
            Thread.Sleep(4000);
            Assert.Equal(0, activeMessageTracker.ActiveMessagesCount);
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelNamespaceNameMatch_ReSends()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();

            var trackingSends = new List<Parcel>();
            var senderConstructor = GetInMemorySender(trackingSends);

            var monitoredChannel = new Channel("ChannelName");
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5),
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                senderConstructor());

            var validParcel = new Parcel { Envelopes = new[] { new NullMessage().ToChanneledMessage(monitoredChannel).ToEnvelope(), } };

            var invalidParcel = new Parcel { Envelopes = new[] { new NullMessage().ToChanneledMessage(new Channel("OtherChannel")).ToEnvelope() } };

            dispatcher.Dispatch(new TrackingCode(), "ValidParcel", validParcel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch(new TrackingCode(), "InvalidParcel", invalidParcel);
            Assert.Equal(1, trackingSends.Count);
        }

        private static Func<IPostOffice> GetInMemorySender(List<Parcel> trackingSends)
        {
            Func<Parcel, TrackingCode> send = parcel =>
                {
                    trackingSends.Add(parcel);
                    return null;
                };

            var ret = A.Fake<IPostOffice>();

            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(IPostOffice.Send))
                .WithReturnType<TrackingCode>()
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Parcel));

            return () => ret;
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelAssemblyQualifiedMatch_ReSends()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();

            var trackingSends = new List<Parcel>();

            var monitoredChannel = new Channel("ChannelName");
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                TypeMatchStrategy.AssemblyQualifiedName,
                TimeSpan.FromSeconds(.5),
                new HarnessStaticDetails(),
                new NullParcelTrackingSystem(),
                new InMemoryActiveMessageTracker(),
                GetInMemorySender(trackingSends)());

            var validParcel = new Parcel { Envelopes = new[] { new NullMessage().ToChanneledMessage(monitoredChannel).ToEnvelope() } };

            var invalidParcel = new Parcel { Envelopes = new[] { new NullMessage().ToChanneledMessage(new Channel("OtherChannel")).ToEnvelope() } };

            dispatcher.Dispatch(new TrackingCode(), "ValidParcel", validParcel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch(new TrackingCode(), "InvalidParcel", invalidParcel);
            Assert.Equal(1, trackingSends.Count);
        }

        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () => 
            GetMessageDispatcher().Dispatch(new TrackingCode(), "Name", null);
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel cannot be null");
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => GetMessageDispatcher().Dispatch(new TrackingCode(), "Name", new Parcel());
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel must contain envelopes");
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => GetMessageDispatcher().Dispatch(new TrackingCode(), "Name", new Parcel { Envelopes = new List<Envelope>() });
            testCode.ShouldThrow<DispatchException>().WithMessage("Parcel must contain envelopes");
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeCompletely_Throws()
        {
            Action testCode = () => GetMessageDispatcher(new[] { new Channel("Channel") }).Dispatch(new TrackingCode(), "Name", new Parcel { Envelopes = new[] { new Envelope(null, null, new Channel("Channel"), null, null) } });

            testCode.ShouldThrow<DispatchException>().WithMessage("Message type not specified in envelope");
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeNamespace_Throws()
        {
            Action testCode =
                () =>
                GetMessageDispatcher(new[]
                                         {
                                             new Channel("Channel")
                                         }).Dispatch(
                    new TrackingCode(), 
                "Name",
                        new Parcel
                            {
                                Envelopes =
                                    new[]
                                        {
                                            new Envelope(
                                                null,
                                                null,
                                                new Channel("Channel"),
                                                null,
                                                new TypeDescription { AssemblyQualifiedName = "Something", Name = "Something" })
                                        }
                            });

            testCode.ShouldThrow<DispatchException>().WithMessage("Message type not specified in envelope");
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeName_Throws()
        {
            Action testCode =
                () =>
                GetMessageDispatcher(new[]
                                         {
                                             new Channel("Channel")
                                         })
                    .Dispatch(
                        new TrackingCode(),
                        "Name",
                        new Parcel
                            {
                                Envelopes =
                                    new[]
                                        {
                                            new Envelope(
                                                null,
                                                null,
                                                new Channel("Channel"),
                                                null,
                                                new TypeDescription { AssemblyQualifiedName = "Something", Namespace = "Something" })
                                        }
                        });
            testCode.ShouldThrow<DispatchException>().WithMessage("Message type not specified in envelope");
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingAssemblyQualifiedType_Throws()
        {
            Action testCode =
                () =>
                GetMessageDispatcher(new[]
                                         {
                                             new Channel("Channel")
                                         })
                    .Dispatch(
                        new TrackingCode(),
                        "Name",
                        new Parcel
                            {
                                Envelopes =
                                    new[]
                                        {
                                            new Envelope(
                                                null,
                                                null,
                                                new Channel("Channel"),
                                                null,
                                                new TypeDescription { Namespace = "Something", Name = "Something" })
                                        }
                        });
            testCode.ShouldThrow<DispatchException>().WithMessage("Message type not specified in envelope");
        }

        [Fact]
        public static void Dispatch_EnvelopeProducingNullMessage_Throws()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();
            Action testCode = () =>
                {
                    GetMessageDispatcher(new[] { new Channel("Channel") }, container)
                        .Dispatch(
                            new TrackingCode(),
                            "Name",
                            new Parcel { Envelopes = new[] { new Envelope(null, null, new Channel("Channel"), null, typeof(NullMessage).ToTypeDescription()) } });
                };

            testCode.ShouldThrow<DispatchException>().WithMessage("First message in parcel deserialized to null");
        }

        [Fact]
        public static void Dispatch_EnvelopeWithUnregisteredType_Throws()
        {
            Action testCode = () =>
                {
                    var channel = new Channel("Channel");

                    var message = new NullMessage();
                    GetMessageDispatcher(new[] { channel })
                        .Dispatch(
                            new TrackingCode(),
                            "Name",
                            new Parcel
                                {
                                    Envelopes =
                                        new[]
                                            {
                                                new Envelope(
                                                    null,
                                                    null,
                                                    new Channel("Channel"),
                                                    Serializer.Serialize(message),
                                                    message.GetType().ToTypeDescription())
                                            }
                                });
                };

            testCode.ShouldThrow<DispatchException>().Where(_ => _.Message.StartsWith("Unable to find handler for message type"));
        }

        [Fact]
        public static void Dispatch_InitialStateRequirement_GetsGenerated()
        {
            StateHandler.StateHistory.Clear();
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(StateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var channel = new Channel("fakeChannel");
            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>(), new[] { channel }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5), new HarnessStaticDetails(), new NullParcelTrackingSystem(), new InMemoryActiveMessageTracker(), new PostOffice(new NullCourier()));
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                     new[] { new Envelope("id", null, channel, messageJson, message.GetType().ToTypeDescription()) })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch(new TrackingCode(), "Parcel Name", parcel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);
        }

        [Fact]
        public static void Dispatch_InitialStateRequirementRunTwice_SecondCallUsesPreviousState()
        {
            StateHandler.StateHistory.Clear();
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(StateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var channel = new Channel("fakeChannel");
            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>(), new[] { channel }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5), new HarnessStaticDetails(), new NullParcelTrackingSystem(), new InMemoryActiveMessageTracker(), new PostOffice(new NullCourier()));
            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                     new[] { new Envelope("id", null, channel, messageJson, message.GetType().ToTypeDescription()) })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch(new TrackingCode(), "Parcel Name", parcel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = true; // this will say that the state is valid and should NOT re-generate
            messageDispatcher.Dispatch(new TrackingCode(), "Parcel Name", parcel);
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
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(StateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var channel = new Channel("fakeChannel");
            var messageDispatcher = GetMessageDispatcher(new[] { channel }, simpleInjectorContainer);

            var parcel = new Parcel
                             {
                                 Id = Guid.NewGuid(),
                                 Envelopes =
                                     new List<Envelope>(
                                     new[] { new Envelope("id", null, channel, messageJson, message.GetType().ToTypeDescription()) })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch(new TrackingCode(), "Parcel Name", parcel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = false; // this will say that the state is NOT valid and should re-generate
            messageDispatcher.Dispatch(new TrackingCode(), "Parcel Name", parcel);
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

        private static MessageDispatcher GetMessageDispatcher(IList<Channel> channels = null, Container container = null)
        {
            if (channels == null)
            {
                channels = new List<Channel>();
            }

            if (container == null)
            {
                container = new Container();
            }

            return new MessageDispatcher(container, new ConcurrentDictionary<Type, object>(), channels, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5), new HarnessStaticDetails(), new NullParcelTrackingSystem(), new InMemoryActiveMessageTracker(), new PostOffice(new NullCourier()));
        }

        public class StateHandler : IHandleMessages<InitialStateMessage>, INeedSharedState<string>
        {
            static StateHandler()
            {
                StateHistory = new Dictionary<string, string>();
            }

            public static Dictionary<string, string> StateHistory { get; set; }

            public static bool ShouldValidate { get; set; }

            public async Task HandleAsync(InitialStateMessage message)
            {
                /* no-op */
                await Task.FromResult<object>(null);
            }

            public string CreateState()
            {
                var state = Guid.NewGuid().ToString().ToUpper();
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

        public class InitialStateMessage : IMessage
        {
            public string Description { get; set; }
        }
    }

    public class MessageOne : IMessage
    {
        public string Description { get; set; }
    }

    public class MessageOneHandler : IHandleMessages<MessageOne>
    {
        public async Task HandleAsync(MessageOne message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public class MessageTwo : IMessage
    {
        public string Description { get; set; }
    }

    public class MessageTwoHandler : IHandleMessages<MessageTwo>
    {
        public async Task HandleAsync(MessageTwo message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public interface IShareNothing : IShare
    {
    }

    public class MessageOneShare : IMessage, IShareNothing
    {
        public string Description { get; set; }
    }

    public class MessageOneShareHandler : IHandleMessages<MessageOneShare>, IShareNothing
    {
        public async Task HandleAsync(MessageOneShare message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }

    public class MessageTwoShare : IMessage, IShareNothing
    {
        public string Description { get; set; }
    }

    public class MessageTwoShareHandler : IHandleMessages<MessageTwoShare>, IShareNothing
    {
        public async Task HandleAsync(MessageTwoShare message)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
    }
}
