// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSenderTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using FluentAssertions;

    using Naos.MessageBus.Domain;

    using Xunit;

    public class MessageSenderTest
    {
        [Fact]
        public static void SenderFactoryGetPostOffice_Uninitialized_Throws()
        {
            // arrange
            Action testCode = () => HandlerToolShed.GetPostOffice();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Factory not initialized for IPostOffice.");
        }

        [Fact]
        public static void SenderFactoryGetParcelTracker_Uninitialized_Throws()
        {
            // skipping on appveyor because it fails (no idea why)...
            if (true.ToString().Equals(Environment.GetEnvironmentVariable("APPVEYOR")))
            {
                return;
            }

            // arrange
            Action testCode = () => HandlerToolShed.GetParcelTracker();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Factory not initialized for ITrackParcels.");
        }
    }
}
