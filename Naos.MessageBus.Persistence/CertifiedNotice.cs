namespace Naos.MessageBus.Persistence
{
    using System;

    using Naos.MessageBus.DataContract;

    public class CertifiedNotice
    {
        public Guid Id { get; set; }

        public string GroupKey { get; set; }

        public Envelope Envelope { get; set; }

        public DateTime DeliveredDateUtc { get; set; }
    }
}