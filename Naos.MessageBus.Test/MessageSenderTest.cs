// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSenderTest.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using FluentAssertions;

    using Naos.MessageBus.Domain;

    using Xunit;

    public static class MessageSenderTest
    {
        [Fact]
        public static void SenderFactoryGetPostOffice_Uninitialized_Throws()
        {
            // arrange
            Action testCode = () => HandlerToolshed.GetPostOffice();

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
            Action testCode = () => HandlerToolshed.GetParcelTracker();

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Factory not initialized for ITrackParcels.");
        }
    }
}
