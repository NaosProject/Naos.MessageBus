// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandlerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;

    using Xunit;

    public class WaitMessageHandlerTests
    {
        [Fact]
        public void Handle_FiveSecondTimeSpan_FiveSecondWait()
        {
            // arrange
            var secondsToWait = 3;
            var message = new WaitMessage { TimeToWait = TimeSpan.FromSeconds(secondsToWait) };
            var handler = new WaitMessageHandler();
            var stopwatch = new Stopwatch();

            // act
            stopwatch.Start();
            handler.Handle(message);
            stopwatch.Stop();

            // assert
            Assert.InRange(stopwatch.Elapsed.TotalSeconds, secondsToWait - 1, secondsToWait + 1);
        }
    }
}
