// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SharedPropertyApplicator.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Hangfire.Harness
{
    using System.Linq;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;

    /// <summary>
    /// Code to handle merging IShare properties.
    /// </summary>
    public static class SharedPropertyApplicator
    {
        /// <summary>
        /// Takes any matching have properties from the handler to the message.
        /// </summary>
        /// <param name="source">Object to find properties on.</param>
        /// <param name="target">Object to apply properties to.</param>
        public static void ApplySharedProperties(IShare source, IShare target)
        {
            if (source == null || target == null)
            {
                throw new SharePropertyException("Neither source nor target can be null");
            }

            // find all interfaces on source that implement IHave
            var sourceType = source.GetType();
            var sourceTypeInterfaces =
                sourceType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces()
                            .Select(inferfaceType => inferfaceType == typeof(IShare))
                            .Any());

            // find matches of those interfaces against
            var targetType = target.GetType();
            var targetTypeInterfaces = targetType.GetInterfaces()
                    .Where(
                        sourceTypeInterface =>
                        sourceTypeInterface.GetInterfaces()
                            .Select(inferfaceType => inferfaceType == typeof(IShare))
                            .Any());
            var typesToDealWith = sourceTypeInterfaces.Intersect(targetTypeInterfaces);

            // squash all the properties from source to target
            foreach (var type in typesToDealWith)
            {
                var properties = type.GetProperties();
                foreach (var prop in properties)
                {
                    var sourceValue = prop.GetValue(source);
                    prop.SetValue(target, sourceValue);
                }
            }
        }
    }
}