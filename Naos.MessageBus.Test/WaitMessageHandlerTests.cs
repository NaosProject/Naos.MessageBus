// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaitMessageHandlerTests.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Naos.MessageBus.Core;
    using Naos.MessageBus.DataContract;

    using Xunit;

    public class WaitMessageHandlerTests
    {
        [Fact]
        public void Handle_ThreeSecondTimeSpan_ThreeSecondWait()
        {
            // arrange
            var secondsToWait = 3;
            var message = new WaitMessage { TimeToWait = TimeSpan.FromSeconds(secondsToWait) };
            var handler = new WaitMessageHandler();
            var stopwatch = new Stopwatch();

            // act
            stopwatch.Start();
            handler.HandleAsync(message).Wait();
            stopwatch.Stop();

            // assert
            Assert.InRange(stopwatch.Elapsed.TotalSeconds, secondsToWait - 1, secondsToWait + 1);
        }
    }
}
