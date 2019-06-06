// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusJsonConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Cron.Serialization.Json;
    using Naos.Serialization.Json;
    using OBeautifulCode.Reflection.Recipes;

    /// <summary>
    /// Implementation for the <see cref="Cron" /> domain.
    /// </summary>
    public class MessageBusJsonConfiguration : JsonConfigurationBase
    {
        /// <inheritdoc />
        protected override IReadOnlyCollection<Type> TypesToAutoRegister => new[]
        {
            typeof(IChannel),
            typeof(TopicBase),
            typeof(TopicStatusReport),
            typeof(MessageBusConnectionConfiguration),
            typeof(MessageBusLaunchConfiguration),
            typeof(HandlerFactoryConfiguration),
        }.Concat(typeof(IMessage).Assembly.GetExportedTypes().Where(_ => _.IsAssignableTo(typeof(IMessage)))).ToList();

        /// <inheritdoc />
        public override IReadOnlyCollection<Type> DependentConfigurationTypes => new[]
        {
            typeof(CronJsonConfiguration),
        };
    }
}