// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusJsonConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using Naos.Cron;
    using Naos.Cron.Serialization.Json;
    using Naos.Serialization.Json;

    /// <summary>
    /// Implementation for the <see cref="Cron" /> domain.
    /// </summary>
    public class MessageBusJsonConfiguration : JsonConfigurationBase
    {
        /// <inheritdoc />
        protected override IReadOnlyCollection<Type> TypesToAutoRegister => new[]
        {
            typeof(IMessage),
            typeof(IChannel),
            typeof(TopicBase),
            typeof(TopicStatusReport),
        };

        /// <inheritdoc />
        public override IReadOnlyCollection<Type> DependentConfigurationTypes => new[]
        {
            typeof(CronJsonConfiguration),
        };
    }
}