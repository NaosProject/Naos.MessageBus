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

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;

    using SimpleInjector;

    using Xunit;

    public class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>()).Dispatch("Name", null);
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel cannot be null", ex.Message);
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>()).Dispatch("Name", new Parcel());
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new ConcurrentDictionary<Type, object>()).Dispatch("Name", new Parcel { Envelopes = new List<Envelope>() });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_InitialStateRequirement_GetsGenerated()
        {
            StateHandler.StateHistory.Clear();
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(StateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>());
            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = new Channel { Name = "fakeChannel" },
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType()
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

            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>());
            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = new Channel { Name = "fakeChannel" },
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType()
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

            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new ConcurrentDictionary<Type, object>());
            var parcel = new Parcel
                             {
                                 Envelopes =
                                     new List<Envelope>(
                                     new[]
                                         {
                                             new Envelope()
                                                 {
                                                     Channel = new Channel { Name = "fakeChannel" },
                                                     MessageAsJson = messageJson,
                                                     MessageType = message.GetType()
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

        public class StateHandler : IHandleMessages<InitialStateMessage>, INeedState<string>
        {
            static StateHandler()
            {
                StateHistory = new Dictionary<string, string>();
            }

            public static Dictionary<string, string> StateHistory { get; set; }

            public static bool ShouldValidate { get; set; }

            public void Handle(InitialStateMessage message)
            {
                /* no-op */
            }

            public string CreateState()
            {
                var state = Guid.NewGuid().ToString().ToUpper();
                StateHistory.Add("GenerateInitialState", state);
                return state;
            }

            public void PreHandle(string initialState)
            {
                StateHistory.Add("SeedInitialState", initialState);
            }

            public bool ValidateState(string initialState)
            {
                StateHistory.Add("ValidateInitialState", initialState);
                return ShouldValidate;
            }
        }

        public class InitialStateMessage : IMessage
        {
            public string Description { get; set; }
        }
    }
}
