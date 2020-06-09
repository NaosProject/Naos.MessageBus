// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageBusJsonSerializationConfiguration.cs" company="Naos Project">
//    Copyright (c) Naos Project 2019. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Cron.Serialization.Json;
    using OBeautifulCode.Serialization;
    using OBeautifulCode.Serialization.Json;
    using OBeautifulCode.Type.Recipes;

    /// <summary>
    /// Implementation for the <see cref="Cron" /> domain.
    /// </summary>
    public class MessageBusJsonSerializationConfiguration : JsonSerializationConfigurationBase
    {
        /// <inheritdoc />
        public override UnregisteredTypeEncounteredStrategy UnregisteredTypeEncounteredStrategy => UnregisteredTypeEncounteredStrategy.Attempt;

        /// <inheritdoc />
        protected override IReadOnlyCollection<TypeToRegisterForJson> TypesToRegisterForJson => new[]
                                                                                                {
                                                                                                    typeof(IChannel).ToTypeToRegisterForJson(),
                                                                                                    typeof(TopicBase).ToTypeToRegisterForJson(),
                                                                                                    typeof(TopicStatusReport).ToTypeToRegisterForJson(),
                                                                                                    typeof(MessageBusConnectionConfiguration).ToTypeToRegisterForJson(),
                                                                                                    typeof(MessageBusLaunchConfiguration).ToTypeToRegisterForJson(),
                                                                                                    typeof(HandlerFactoryConfiguration).ToTypeToRegisterForJson(),
                                                                                                    typeof(UnitOfWorkResult).ToTypeToRegisterForJson(),
                                                                                                }.Concat(
                                                                                                      typeof(IMessage)
                                                                                                         .Assembly.GetExportedTypes()
                                                                                                         .Where(
                                                                                                              _ => !_.IsGenericType
                                                                                                                && _.IsAssignableTo(typeof(IMessage)) && _ != typeof(IMessage))
                                                                                                         .Select(_ => _.ToTypeToRegisterForJson()))
                                                                                                 .ToList();

        /// <inheritdoc />
        protected override IReadOnlyCollection<JsonSerializationConfigurationType> DependentJsonSerializationConfigurationTypes => new[]
        {
            typeof(CronJsonSerializationConfiguration).ToJsonSerializationConfigurationType(),
        };
    }
}
