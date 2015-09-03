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
    using System.Threading.Tasks;

    using ImpromptuInterface;
    using ImpromptuInterface.Dynamic;

    using Naos.Cron;
    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    using Xunit;

    public class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_DispatchingMethodToWrongChannelNamespaceNameMatch_ReSends()
        {
            var container = new Container();
            container.Register<IHandleMessages<NullMessage>, NullMessageHandler>();

            var trackingSends = new List<Parcel>();
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
                                                     MessageTypeNamespace = typeof(NullMessage).Namespace,
                                                     MessageTypeName = typeof(NullMessage).Name,
                                                     MessageTypeAssemblyQualifiedName = typeof(NullMessage).AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = typeof(NullMessage).Namespace,
                                                     MessageTypeName = typeof(NullMessage).Name,
                                                     MessageTypeAssemblyQualifiedName = typeof(NullMessage).AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = typeof(NullMessage).Namespace,
                                                     MessageTypeName = typeof(NullMessage).Name,
                                                     MessageTypeAssemblyQualifiedName = typeof(NullMessage).AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = typeof(NullMessage).Namespace,
                                                     MessageTypeName = typeof(NullMessage).Name,
                                                     MessageTypeAssemblyQualifiedName = typeof(NullMessage).AssemblyQualifiedName,
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
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { MessageTypeName = "Name", Channel = new Channel { Name = "Channel" } } } });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message type not specified in envelope", ex.Message);
        }

        [Fact]
        public static void Dispatch_EnvelopeMissingTypeName_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>(), new[] { new Channel { Name = "Channel" } }, TypeMatchStrategy.NamespaceAndName, TimeSpan.FromSeconds(.5)).Dispatch("Name", new Parcel { Envelopes = new[] { new Envelope { MessageTypeNamespace = "Namespace", Channel = new Channel { Name = "Channel" } } } });
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
                                                        MessageTypeNamespace =
                                                            typeof(NullMessage).Namespace,
                                                        MessageTypeName = typeof(NullMessage).Name,
                                                        MessageTypeAssemblyQualifiedName =
                                                            typeof(NullMessage).AssemblyQualifiedName,
                                                    }
                                            }
                                });
                };

            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Message deserialized to null", ex.Message);
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
                                                              MessageTypeNamespace = typeof(NullMessage).Namespace,
                                                              MessageTypeName = typeof(NullMessage).Name,
                                                              MessageTypeAssemblyQualifiedName =
                                                                  typeof(NullMessage).AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = message.GetType().Namespace,
                                                     MessageTypeName = message.GetType().Name,
                                                     MessageTypeAssemblyQualifiedName = message.GetType().AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = message.GetType().Namespace,
                                                     MessageTypeName = message.GetType().Name,
                                                     MessageTypeAssemblyQualifiedName = message.GetType().AssemblyQualifiedName,
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
                                                     MessageTypeNamespace = message.GetType().Namespace,
                                                     MessageTypeName = message.GetType().Name,
                                                     MessageTypeAssemblyQualifiedName = message.GetType().AssemblyQualifiedName,
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
