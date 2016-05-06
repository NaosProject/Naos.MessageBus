// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullParcelSender.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Test
{
    using System;

    using Naos.Cron;
    using Naos.MessageBus.Domain;

    public class NullParcelSender : ISendParcels
    {
        public TrackingCode Send(IMessage message, Channel channel)
        {
            return new TrackingCode();
        }

        public TrackingCode Send(MessageSequence messageSequence)
        {
            return new TrackingCode();
        }

        public TrackingCode Send(Parcel parcel)
        {
            return new TrackingCode();
        }

        public TrackingCode SendRecurring(Parcel parcel, ScheduleBase recurringSchedule)
        {
            return new TrackingCode();
        }

        public TrackingCode SendRecurring(IMessage message, Channel channel, ScheduleBase recurringSchedule)
        {
            return new TrackingCode();
        }

        public TrackingCode SendRecurring(MessageSequence messageSequence, ScheduleBase recurringSchedule)
        {
            return new TrackingCode();
        }
    }
}