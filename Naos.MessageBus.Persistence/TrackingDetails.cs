namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public class TrackingDetails
    {
        public HarnessDetails Recipient { get; internal set; }

        public Channel Address { get; internal set; }

        public Exception Exception { get; internal set; }

        public ParcelStatus Status { get; internal set; }

        public CertifiedNotice Certified { get; internal set; }

        public Envelope Envelope { get; internal set; }
    }
}