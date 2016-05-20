// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Factory.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FakeItEasy;

    using Naos.MessageBus.Domain;

    internal class Factory
    {
        public static Func<ICourier> GetInMemoryCourier(List<Crate> trackingSends)
        {
            Action<Crate> send = trackingSends.Add;

            var ret = A.Fake<ICourier>();
            A.CallTo(ret)
                .Where(call => call.Method.Name == nameof(ICourier.Send))
                .Invokes(call => send(call.Arguments.FirstOrDefault() as Crate));
            return () => ret;
        }
    }
}