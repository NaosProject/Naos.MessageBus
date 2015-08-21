﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageDispatcher.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Its.Log.Instrumentation;

    using Naos.Cron;
    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.DataContract.Exceptions;
    using Naos.MessageBus.HandlingContract;
    using Naos.MessageBus.SendingContract;

    using SimpleInjector;

    /// <inheritdoc />
    public class MessageDispatcher : IDispatchMessages
    {
        private readonly Container simpleInjectorContainer;

        private readonly ConcurrentDictionary<Type, object> handlerSharedStateMap;

        private readonly ICollection<Channel> servicedChannels;

        private readonly MessageTypeMatchStrategy messageTypeMatchStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDispatcher"/> class.
        /// </summary>
        /// <param name="simpleInjectorContainer">DI container to use for looking up handlers.</param>
        /// <param name="handlerSharedStateMap">Initial state dictionary for handlers the require state to be seeded.</param>
        /// <param name="servicedChannels">Channels being services by this dispatcher.</param>
        /// <param name="messageTypeMatchStrategy">Message type match strategy for use when selecting a handler.</param>
        public MessageDispatcher(Container simpleInjectorContainer, ConcurrentDictionary<Type, object> handlerSharedStateMap, ICollection<Channel> servicedChannels, MessageTypeMatchStrategy messageTypeMatchStrategy)
        {
            this.simpleInjectorContainer = simpleInjectorContainer;
            this.handlerSharedStateMap = handlerSharedStateMap;
            this.servicedChannels = servicedChannels;
            this.messageTypeMatchStrategy = messageTypeMatchStrategy;
        }

        /// <inheritdoc />
        public void Dispatch(string displayName, Parcel parcel)
        {
            if (parcel == null)
            {
                throw new DispatchException("Parcel cannot be null");
            }

            if (parcel.Envelopes == null || !parcel.Envelopes.Any())
            {
                throw new DispatchException("Parcel must contain envelopes");
            }

            // make sure the message was routed correctly (if not then reroute)
            if (this.servicedChannels.SingleOrDefault(_ => _.Name == parcel.Envelopes.First().Channel.Name) == null)
            {
                var rerouteMessageSender = this.simpleInjectorContainer.GetInstance<ISendMessages>();

                // any schedule should already be set and NOT reset...
                rerouteMessageSender.Send(parcel);

                return;
            }

            Func<string, string, string, Type> getTypeForLocalVersion = (typeNamespace, typeName, typeAssemblyQualifiedName) =>
                {
                    var registeredHandlers =
                        this.simpleInjectorContainer.GetCurrentRegistrations()
                            .Select(_ => _.Registration.ImplementationType)
                            .ToList()
                            .GetTypeMapsOfImplementersOfGenericType(typeof(IHandleMessages<>));

                    foreach (var registeredHandler in registeredHandlers)
                    {
                        var messageTypeFromRegistered = registeredHandler.InterfaceType.GenericTypeArguments.Single();
                        if (this.messageTypeMatchStrategy == MessageTypeMatchStrategy.NamespaceAndName
                            && messageTypeFromRegistered.Namespace == typeNamespace
                            && messageTypeFromRegistered.Name == typeName)
                        {
                            return messageTypeFromRegistered;
                        }

                        if (this.messageTypeMatchStrategy == MessageTypeMatchStrategy.AssemblyQualifiedName
                            && messageTypeFromRegistered.AssemblyQualifiedName == typeAssemblyQualifiedName)
                        {
                            return messageTypeFromRegistered;
                        }
                    }

                    throw new DispatchException("Unable to find handler for message type; Namespace: " + typeNamespace + ", Name: " + typeName);
                };

            var channeledMessages = parcel.Envelopes.Select(
                _ =>
                    {
                        if (string.IsNullOrEmpty(_.MessageTypeNamespace) || string.IsNullOrEmpty(_.MessageTypeName))
                        {
                            throw new DispatchException("Message type not specified in envelope");
                        }

                        var ret = new ChanneledMessage
                                      {
                                          Message =
                                              (IMessage)
                                              Serializer.Deserialize(
                                                  getTypeForLocalVersion(_.MessageTypeNamespace, _.MessageTypeName, _.MessageTypeAssemblyQualifiedName),
                                                  _.MessageAsJson),
                                          Channel = _.Channel
                                      };

                        if (ret.Message == null)
                        {
                            throw new DispatchException("Message deserialized to null");
                        }

                        return ret;
                    }).ToList();

            var firstMessage = channeledMessages.First().Message;
            var remainingChanneledMessages = channeledMessages.Skip(1).ToList();

            var messageType = firstMessage.GetType();
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);

            Log.Write(() => "Attempting to get handler for type: " + handlerType.FullName);
            object handler;
            var matchingRegistration =
                this.simpleInjectorContainer.GetCurrentRegistrations()
                    .SingleOrDefault(_ => _.ServiceType.FullName == handlerType.FullName);
            if (matchingRegistration != null)
            {
                // DON'T use "handler = matchingRegistration.GetInstance();" as it will suppress the Fusion Log error when there is a contract version mismatch...
                handler = Activator.CreateInstance(matchingRegistration.Registration.ImplementationType);
            }
            else
            {
                throw new ApplicationException("Could not find type in container: " + handlerType.FullName);
            }

            var handlerActualType = handler.GetType();
            Log.Write(() => "Loaded handler: " + handlerActualType.FullName);

            var handlerInterfaces = handlerActualType.GetInterfaces();
            if (handlerInterfaces.Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(INeedSharedState<>)))
            {
                using (var activity = Log.Enter(() => new { HandlerType = handlerActualType, Handler = handler }))
                {
                    try
                    {
                        activity.Trace(() => "Detected need for state management.");

                        // this will only get set to false if there is existing state AND it is valid
                        var stateNeedsCreation = true;

                        object state;
                        var haveStateToValidateAndUse = this.handlerSharedStateMap.TryGetValue(
                            handlerActualType,
                            out state);
                        if (haveStateToValidateAndUse)
                        {
                            activity.Trace(() => "Found pre-generated initial state.");
                            var validateMethodInfo = handlerActualType.GetMethod("IsStateStillValid");
                            var validRaw = validateMethodInfo.Invoke(handler, new[] { state });
                            var valid = (bool)validRaw;
                            if (!valid)
                            {
                                activity.Trace(() => "State was found to be invalid, not using.");
                                object removalOutput;
                                var removedThisTime = this.handlerSharedStateMap.TryRemove(
                                    handlerActualType,
                                    out removalOutput);
                                if (removedThisTime)
                                {
                                    // this is where you would dispose if it were disposable DO NOT DO THIS!!! this object could be in live handlers that are not finished yet...
                                    activity.Trace(() => "Invalidated state and removed from dictionary.");
                                }
                                else
                                {
                                    activity.Trace(() => "Invalidated state but was already removed from dictionary.");
                                }
                            }
                            else
                            {
                                activity.Trace(() => "Initial state was found to be valid.");
                                stateNeedsCreation = false;
                            }
                        }

                        // this is the performance gain in case state creation is expensive...
                        if (stateNeedsCreation)
                        {
                            activity.Trace(
                                () =>
                                "No pre-existing state or it is invalid, creating new state for this use and future uses.");
                            var getInitialStateMethod = handlerActualType.GetMethod("CreateState");
                            state = getInitialStateMethod.Invoke(handler, new object[0]);

                            activity.Trace(() => "Adding state to tracking map for future use.");
                            this.handlerSharedStateMap.AddOrUpdate(
                                handlerActualType,
                                state,
                                (key, existingStateInDictionary) =>
                                    {
                                        activity.Trace(() => "State already exists in map, updating with new state.");
                                        return state;
                                    });
                        }

                        activity.Trace(() => "Applying state to handler.");
                        var seedMethodInfo = handlerActualType.GetMethod("PreHandleWithState");
                        seedMethodInfo.Invoke(handler, new[] { state });

                        activity.Confirm(() => "Finished state management work.");
                    }
                    catch (Exception ex)
                    {
                        activity.Trace(() => ex);
                        throw;
                    }
                }
            }

            using (var activity = Log.Enter(() => new { Message = firstMessage, Handler = handler }))
            {
                try
                {
                    activity.Trace(() => "Handling message (calling Handle on selected Handler).");
                    var methodInfo = handlerType.GetMethod("Handle");
                    methodInfo.Invoke(handler, new object[] { firstMessage });
                    activity.Confirm(() => "Successfully handled message.");
                }
                catch (Exception ex)
                {
                    activity.Trace(() => ex);
                    throw;
                }
            }

            if (remainingChanneledMessages.Any())
            {
                using (var activity = Log.Enter(() => new { RemainingMessage = remainingChanneledMessages }))
                {
                    try
                    {
                        activity.Trace(() => "Found remaining messages in sequence.");

                        var handlerAsShare = handler as IShare;
                        foreach (var channeledMessageToShareTo in remainingChanneledMessages)
                        {
                            var messageToShareTo = channeledMessageToShareTo.Message as IShare;
                            if (handlerAsShare != null && messageToShareTo != null)
                            {
                                activity.Trace(
                                    () =>
                                    "Discovered need to share, sharing applicable properties to remaining messages in sequence.");

                                // CHANGES STATE: this will pass IShare properties from the handler to all messages in the sequence before re-sending the trimmed sequence
                                SharedPropertyApplicator.ApplySharedProperties(handlerAsShare, messageToShareTo);
                            }
                        }

                        activity.Trace(() => "Sending remaining messages in sequence.");
                        var sender = this.simpleInjectorContainer.GetInstance<ISendMessages>();
                        var remainingMessageSequence = new MessageSequence
                                                           {
                                                               Id = parcel.Id, // persist the batch ID for collation
                                                               ChanneledMessages =
                                                                   remainingChanneledMessages
                                                           };

                        sender.Send(remainingMessageSequence);

                        activity.Confirm(() => "Finished sending remaining messages in sequence.");
                    }
                    catch (Exception ex)
                    {
                        activity.Trace(() => ex);
                        throw;
                    }
                }
            }
        }
    }
}