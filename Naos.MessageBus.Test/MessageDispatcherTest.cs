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
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.Hangfire.Harness;

    using SimpleInjector;

    using Xunit;

    public class MessageDispatcherTest
    {
        [Fact]
        public static void Dispatch_NullParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new Dictionary<Type, object>()).Dispatch("Name", null);
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel cannot be null", ex.Message);
        }

        [Fact]
        public static void Dispatch_NullEnvelopesInParcel_Throws()
        {
            Action testCode = () => new MessageDispatcher(new Container(), new Dictionary<Type, object>()).Dispatch("Name", new Parcel());
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_NoEnvelopesInParcel_Throws()
        {
            Action testCode =
                () => new MessageDispatcher(new Container(), new Dictionary<Type, object>()).Dispatch("Name", new Parcel { Envelopes = new List<Envelope>() });
            var ex = Assert.Throws<DispatchException>(testCode);
            Assert.Equal("Parcel must contain envelopes", ex.Message);
        }

        [Fact]
        public static void Dispatch_InitialStateRequirement_GetsGenerated()
        {
            var simpleInjectorContainer = new Container();
            simpleInjectorContainer.Register(typeof(IHandleMessages<InitialStateMessage>), typeof(InitialStateHandler));
            var message = new InitialStateMessage();
            var messageJson = Serializer.Serialize(message);

            var messageDispatcher = new MessageDispatcher(simpleInjectorContainer, new Dictionary<Type, object>());
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

            Assert.Empty(InitialStateHandler.StateHistory);
            messageDispatcher.Dispatch("Parcel Name", parcel);
            Assert.Equal(2, InitialStateHandler.StateHistory.Count);
            Assert.Equal(
                InitialStateHandler.StateHistory["GenerateInitialState"],
                InitialStateHandler.StateHistory["SeedInitialState"]);
        }

        public class InitialStateHandler : IHandleMessages<InitialStateMessage>, INeedInitialState<string>
        {
            static InitialStateHandler()
            {
                StateHistory = new Dictionary<string, string>();
            }

            public static Dictionary<string, string> StateHistory { get; set; }

            public static bool ShouldValidate { get; set; }

            public void Handle(InitialStateMessage message)
            {
                /* no-op */
            }

            public string GenerateInitialState()
            {
                var state = Guid.NewGuid().ToString().ToUpper();
                StateHistory.Add("GenerateInitialState", state);
                return state;
            }

            public void SeedInitialState(string initialState)
            {
                StateHistory.Add("SeedInitialState", initialState);
            }

            public bool ValidateInitialState(string initialState)
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
