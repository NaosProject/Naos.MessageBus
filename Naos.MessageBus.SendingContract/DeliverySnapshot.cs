namespace Naos.MessageBus.SendingContract
{
    using System;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.DataContract;

    public class DeliverySnapshot : ISnapshot
    {
        public Guid AggregateId { get; set; }

        public long Version { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public string AggregateTypeName { get; set; }

        public BloomFilter ETags { get; set; }

        public ParcelStatus Status { get; set; }

        public Exception Exception { get; set; }

        public Channel Address { get; set; }

        public HarnessDetails Recipient { get; set; }

        public Parcel Parcel { get; set; }
    }
}