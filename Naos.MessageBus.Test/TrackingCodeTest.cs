// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackingCodeTest.cs" company="Naos">
//    Copyright (c) Naos 2017. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using Naos.MessageBus.Domain;

    using Xunit;

    public static class TrackingCodeTest
    {
        [Fact]
        public static void Equal_AreEqual()
        {
            var firstParcelId = Guid.NewGuid();
            var firstCode = Guid.NewGuid().ToString().ToUpperInvariant();

            var first = new TrackingCode { EnvelopeId = firstCode, ParcelId = firstParcelId };

            var secondParcelId = firstParcelId;
            var secondCode = firstCode;
            var second = new TrackingCode { EnvelopeId = secondCode, ParcelId = secondParcelId };

            Assert.True(first == second);
            Assert.False(first != second);
            Assert.True(first.Equals(second));
            Assert.True(first.Equals((object)second));
            Assert.Equal(first, second);
            Assert.Equal(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_ParcelId()
        {
            var firstParcelId = Guid.NewGuid();
            var firstCode = Guid.NewGuid().ToString().ToUpperInvariant();
            var first = new TrackingCode { EnvelopeId = firstCode, ParcelId = firstParcelId };

            var secondParcelId = Guid.NewGuid();
            var secondCode = firstCode;
            var second = new TrackingCode { EnvelopeId = secondCode, ParcelId = secondParcelId };

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Fact]
        public static void NotEqualAreNotEqual_Code()
        {
            var firstParcelId = Guid.NewGuid();
            var firstCode = Guid.NewGuid().ToString().ToUpperInvariant();
            var first = new TrackingCode { EnvelopeId = firstCode, ParcelId = firstParcelId };

            var secondParcelId = firstParcelId;
            var secondCode = Guid.NewGuid().ToString().ToUpperInvariant();
            var second = new TrackingCode { EnvelopeId = secondCode, ParcelId = secondParcelId };

            Assert.False(first == second);
            Assert.True(first != second);
            Assert.False(first.Equals(second));
            Assert.False(first.Equals((object)second));
            Assert.NotEqual(first, second);
            Assert.NotEqual(first.GetHashCode(), second.GetHashCode());
        }
    }
}
