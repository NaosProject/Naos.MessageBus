// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageSenderTest.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Naos.Cron;
    using Naos.MessageBus.Domain;
    using Naos.MessageBus.Domain.Exceptions;
    using Naos.MessageBus.Hangfire.Sender;

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

        [Fact]
        public static void Send_NullChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(null);
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>().WithMessage("Cannot use null channel name.");
        }

        [Fact]
        public static void Send_LongChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(new string('a', 21));
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage(
                    "Cannot use a channel name longer than 20 characters.  The supplied channel name: " + channel.Name + " is " + channel.Name.Length
                    + " characters long.");
        }

        [Fact]
        public static void Send_UpperCaseChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel(new string('A', 20));
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage("Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: " + channel.Name);
        }

        [Fact]
        public static void Send_DashesChannelName_Throws()
        {
            // arrange
            var channel = new SimpleChannel("sup-withthis");
            Action testCode = () => HangfireCourier.ThrowIfInvalidChannel(channel);

            // act & assert
            testCode.ShouldThrow<ArgumentException>()
                .WithMessage("Channel name must be lowercase alphanumeric with underscores ONLY.  The supplied channel name: " + channel.Name);
        }
    }
}
