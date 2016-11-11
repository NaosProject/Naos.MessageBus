// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrowsExceptionMessageHandler.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Its.Log.Instrumentation;

    using Naos.MessageBus.Domain;

    /// <inheritdoc />
    public class ThrowsExceptionMessageHandler : IHandleMessages<ThrowsExceptionMessage>
    {
        /// <inheritdoc />
        public async Task HandleAsync(ThrowsExceptionMessage message)
        {
            Log.Write(new LogEntry(message.Description, message));

            var type = ResolveTypeDescriptionFromLoadedTypes(message.ExceptionToThrowType, message.TypeMatchStrategy);
            var exception = message.ExceptionToThrowJson.FromJson(type) as Exception;
            if (exception == null)
            {
                throw new NullReferenceException("Failed to deserialize the exception correctly.");
            }

            await Task.Run(() => { throw exception; });
        }

        private static Type ResolveTypeDescriptionFromLoadedTypes(TypeDescription typeDescription, TypeMatchStrategy typeMatchStrategy)
        {
            var typeComparer = new TypeComparer(typeMatchStrategy);

            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_ => _.GetTypes()).ToList();

            return allTypes.SingleOrDefault(_ => typeComparer.Equals(_.ToTypeDescription(), typeDescription));
        }
    }
}