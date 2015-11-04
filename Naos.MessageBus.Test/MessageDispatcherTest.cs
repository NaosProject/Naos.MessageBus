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
    using System.Dynamic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ImpromptuInterface;
    using ImpromptuInterface.Dynamic;

    using Naos.Cron;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

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
            container.Register(senderConstructor);

            var tracker = new InMemoryJobTracker();
            var channel = new Channel { Name = "el-channel" };

            container.Register<IHandleMessages<FirstEnumMessage>, FirstEnumHandler>();
            container.Register<IHandleMessages<SecondEnumMessage>, SecondEnumHandler>();

            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5),
                tracker.IncrementActiveJobs,
                tracker.DecrementActiveJobs);

            var firstMessage = new FirstEnumMessage() { Description = "RunMe 1", SeedValue = MyEnum.OtherValue };
            var secondMessage = new SecondEnumMessage() { Description = "RunMe 2" };
            var thirdMessage = new SecondEnumMessage() { Description = "RunMe 3" };
            var fourthMessage = new SecondEnumMessage() { Description = "RunMe 4" };
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
                                                          },
                                                      new ChanneledMessage
                                                          {
                                                              Channel = channel,
                                                              Message = fourthMessage
                                                          }
                                                  }
                                      };

            var envelopesFromSequence = messageSequence.ChanneledMessages.Select(
                channeledMessage =>
                    {
                        var messageType = channeledMessage.Message.GetType();
                        return new Envelope()
                                   {
                                       Description = channeledMessage.Message.Description,
                                       MessageAsJson =
                                           Hangfire.Sender.Serializer.Serialize(channeledMessage.Message),
                                       MessageType = messageType.ToTypeDescription(),
                                       Channel = channeledMessage.Channel
                                   };
                    }).ToList();

            var parcel = new Parcel { Envelopes = envelopesFromSequence };

            dispatcher.Dispatch("First Message", parcel);

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

            dispatcher.Dispatch("Second Message", newParcel);

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
            // make the second message a fake type and run sequence verify first message succeeds...
        }

        [Fact]
        public static void Dispatch_IncrementsAndDecrementsTracker()
        {
            var tracker = new InMemoryJobTracker();
            var channel = new Channel { Name = "el-channel" };
            var container = new Container();
            container.Register<IHandleMessages<WaitMessage>, WaitMessageHandler>();
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5),
                tracker.IncrementActiveJobs,
                tracker.DecrementActiveJobs);

            var message = new WaitMessage { Description = "RunMe", TimeToWait = TimeSpan.FromSeconds(3) };
            var jsonMessage = Serializer.Serialize(message);
            var envelope = new Envelope
                               {
                                   Channel = channel,
                                   Description = "RunMe",
                                   MessageAsJson = jsonMessage,
                                   MessageType = message.GetType().ToTypeDescription(),
                               };

            Assert.Equal(0, tracker.ActiveJobsCount);
            ThreadPool.QueueUserWorkItem(
                state => dispatcher.Dispatch("RunMe", new Parcel { Envelopes = new[] { envelope } }));
            Thread.Sleep(2000);
            Assert.Equal(1, tracker.ActiveJobsCount);
            Thread.Sleep(4000);
            Assert.Equal(0, tracker.ActiveJobsCount);
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelNamespaceNameMatch_ReSends()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();

            var trackingSends = new List<Parcel>();
            var senderConstructor = GetInMemorySender(trackingSends);
            container.Register(senderConstructor);

            var monitoredChannel = new Channel { Name = "ChannelName" };
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                TypeMatchStrategy.NamespaceAndName,
                TimeSpan.FromSeconds(.5));

            var validParcel = new Parcel()
                             {
                                 Envelopes =
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = monitoredChannel,
                                                     MessageType = typeof(NullMessage).ToTypeDescription(),
                                                     MessageAsJson = Serializer.Serialize(new NullMessage())
                                                 }
                                         }
                             };

            var invalidParcel = new Parcel()
                             {
                                 Envelopes =
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = new Channel() { Name = "OtherChannel" },
                                                     MessageType = typeof(NullMessage).ToTypeDescription(),
                                                     MessageAsJson = Serializer.Serialize(new NullMessage())
                                                 }
                                         }
                             };

            dispatcher.Dispatch("ValidParcel", validParcel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch("InvalidParcel", invalidParcel);
            Assert.Equal(1, trackingSends.Count);
        }

        private static Func<ISendMessages> GetInMemorySender(List<Parcel> trackingSends)
        {
            Func<ISendMessages> senderConstructor = () =>
                {
                    dynamic dynamicObject = new ExpandoObject();
                    dynamicObject.Send = Return<TrackingCode>.Arguments<Parcel>(
                        (parcel) =>
                            {
                                trackingSends.Add(parcel);
                                return null;
                            });

                    ISendMessages ret = Impromptu.ActLike(dynamicObject);
                    return ret;
                };
            return senderConstructor;
        }

        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelAssemblyQualifiedMatch_ReSends()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();

            var trackingSends = new List<Parcel>();
            Func<ISendMessages> senderConstructor = () =>
            {
                dynamic dynamicObject = new ExpandoObject();
                dynamicObject.SendRecurring = Return<TrackingCode>.Arguments<Parcel, ScheduleBase>(
                    (parcel, schedule) =>
                        {
                            trackingSends.Add(parcel);
                            return null;
                        });

                dynamicObject.Send = Return<TrackingCode>.Arguments<Parcel>(
                    (parcel) =>
                        {
                            trackingSends.Add(parcel);
                            return null;
                        });

                ISendMessages ret = Impromptu.ActLike(dynamicObject);
                return ret;
            };

            container.Register(senderConstructor);
            var monitoredChannel = new Channel { Name = "ChannelName" };
            var dispatcher = new MessageDispatcher(
                container,
                new ConcurrentDictionary<Type, object>(),
                new[] { monitoredChannel },
                TypeMatchStrategy.AssemblyQualifiedName,
                TimeSpan.FromSeconds(.5));

            var validParcel = new Parcel()
            {
                Envelopes =
                    new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = monitoredChannel,
                                                     MessageType = typeof(NullMessage).ToTypeDescription(),
                                                     MessageAsJson = Serializer.Serialize(new NullMessage())
                                                 }
                                         }
            };

            var invalidParcel = new Parcel()
            {
                Envelopes =
                    new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = new Channel() { Name = "OtherChannel" },
                                                     MessageType = typeof(NullMessage).ToTypeDescription(),
                                                     MessageAsJson = Serializer.Serialize(new NullMessage())
                                                 }
                                         }
            };

            dispatcher.Dispatch("ValidParcel", validParcel);
            Assert.Equal(0, trackingSends.Count);
            dispatcher.Dispatch("InvalidParcel", invalidParcel);
            Assert.Equal(1, trackingSends.Count);
        }

        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new List<Channel>(), TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", null);
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel cannot be null", ex.Message);
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new List<Channel>(), TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel());
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new List<Channel>(), TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new List<Envelope>() });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeCompletely_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { Channel = new Channel { Name = "Channel" } } } });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message type not specified in envelope", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeNamespace_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { MessageType = new TypeDescription { AssemblyQualifiedName = "Something", Name = "Something" }, Channel = new Channel { Name = "Channel" } } } });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message type not specified in envelope", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeName_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { MessageType = new TypeDescription { AssemblyQualifiedName = "Something", Namespace = "Something" }, Channel = new Channel { Name = "Channel" } } } });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message type not specified in envelope", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingAssemblyQualifiedType_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { MessageType = new TypeDescription { Name = "Something", Namespace = "Something" }, Channel = new Channel { Name = "Channel" } } } });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message type not specified in envelope", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeProducingNullMessage_Throws()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();
            Action testCode = () =>
                {
                    new MessageDispatcher(
                        container,
                        new ConcurrentDictionary<Type, object>(),
                        new[] { new Channel { Name = "Channel" } },
                        TypeMatchStrategy.NamespaceAndName, 
                        TimeSpan.FromSeconds(.5)).Dispatch(
                            "Name",
                            new Parcel
                                {
                                    Envelopes =
                                        new[]
                                            {
                                                new Envelope
                                                    {
                                                        Channel = new Channel { Name = "Channel" },
                                                     MessageType = typeof(NullMessage).ToTypeDescription(),
                                                    }
                                            }
                                });
                };

            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("First message in parcel deserialized to null", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeWithUnregisteredType_Throws()
        {
            Action testCode =
                () =>
                    {
                        var channel = new Channel { Name = "Channel" };

                        new MessageDispatcher(
                              new Container(), 
                              new ConcurrentDictionary<Type, object>(), 
                              new[] { channel },
                              TypeMatchStrategy.NamespaceAndName, 
                              TimeSpan.FromSeconds(.5))
                              .Dispatch(
                                  "Name",
                                  new Parcel
                                      {
                                          Envelopes =
                                              new[]
                                                  {
                                                      new Envelope
                                                          {
                                                              Channel = channel,
                                                              MessageType = typeof(NullMessage).ToTypeDescription()
                                                          }
                                                  }
                                      });
                    };

            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.True(ex.Message.StartsWith("Unable to find handler for message type"), ex.Message);
        }

        [Fact]
        public static void Dispatch_InitialStateRequirement_GetsGenerated()
        {
            StateHandler.StateHistory.Clear();
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(StateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var channel = new Channel { Name = "fakeChannel" };
            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>(), new[] { channel }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5));
            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = channel,
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType().ToTypeDescription()
                                                 }
                                         })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", parcel);
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

            var channel = new Channel { Name = "fakeChannel" };
            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>(), new[] { channel }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5));
            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = channel,
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType().ToTypeDescription()
                                                 }
                                         })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", parcel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = true; // this will say that the state is valid and should NOT re-generate
            messageDispatcher.Dispatch("Parcel Name", parcel);
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

            var channel = new Channel { Name = "fakeChannel" };
            var messageDispatcher = new MessageDispatcher(
                simpleInjectorContainer,
                new ConcurrentDictionary<Type, object>(),
                new[] { channel },
                TypeMatchStrategy.NamespaceAndName, 
                TimeSpan.FromSeconds(.5));

            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = channel,
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType().ToTypeDescription()
                                                 }
                                         })
                             };

            Assert.Empty(StateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", parcel);
            Assert.Equal(2, StateHandler.StateHistory.Count);
            Assert.Equal(
                StateHandler.StateHistory["GenerateInitialState"],
                StateHandler.StateHistory["SeedInitialState"]);

            StateHandler.StateHistory.Clear();
            StateHandler.ShouldValidate = false; // this will say that the state is NOT valid and should re-generate
            messageDispatcher.Dispatch("Parcel Name", parcel);
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
}
