// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryCourierAndTracker.cs" company="Naos">
//   Copyright 2015 Naos
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    /// <summary>
    /// In memory implementation of a parcel tracker.
    /// </summary>
    public class InMemoryPostmaster : IPostmaster, ITrackParcels
    {
        private readonly ConcurrentDictionary<TrackingCode, Delivery> deliveries = new ConcurrentDictionary<TrackingCode, Delivery>();

        /// <inheritdoc />
        public void TrackSent(TrackingCode trackingCode, Parcel parcel, IReadOnlyDictionary<string, string> metadata)
        {
            //should the aggregate be natively worked with or hidden behind an interface?

            //who should be newing up the aggregate?
            var aggregate = new Delivery();
            this.deliveries.AddOrUpdate(trackingCode, aggregate, (code, aggregateInput) => aggregateInput);

            this.GetDeliveryByTrackingCode(trackingCode).EnactCommand(new SendCommand { Parcel = parcel });
        }

        /// <inheritdoc />
        public void TrackAddressed(TrackingCode trackingCode, Channel assignedChannel)
        {
            this.GetDeliveryByTrackingCode(trackingCode).EnactCommand(new AddressCommand { Address = assignedChannel });
        }

        /// <inheritdoc />
        public void TrackAttemptingDelivery(TrackingCode trackingCode, HarnessStaticDetails harnessStaticDetails, HarnessDynamicDetails harnessDynamicDetails)
        {
            this.GetDeliveryByTrackingCode(trackingCode).EnactCommand(new AttemptCommand { Recipient = new HarnessDetails { StaticDetails = harnessStaticDetails, Details = harnessDynamicDetails } });
        }

        /// <inheritdoc />
        public void TrackAccepted(TrackingCode trackingCode)
        {
            this.GetDeliveryByTrackingCode(trackingCode).EnactCommand(new AcceptCommand());
        }

        public void CreateAggregate(TrackingCode trackingCode)
        {
            throw new NotImplementedException();
        }

        public Delivery CreateAndGetAggregate(TrackingCode trackingCode)
        {
            throw new NotImplementedException();
        }

        public Delivery GetAggregate(TrackingCode trackingCode)
        {
            throw new NotImplementedException();
        }

        public void Persist(Delivery aggregate)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void TrackRejected(TrackingCode trackingCode, Exception exception)
        {
            this.GetDeliveryByTrackingCode(trackingCode).EnactCommand(new RejectCommand { Exception = exception });
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DeliverySnapshot> Track(IReadOnlyCollection<TrackingCode> trackingCodes)
        {
            var ret = this.deliveries.Where(_ => trackingCodes.Contains(_.Key)).Select(_ => _.Value).ToList();

            return ConvertAggregateToSnapshot(ret);
        }

        private Delivery GetDeliveryByTrackingCode(TrackingCode trackingCode)
        {
            //how should i get aggregates to enact commands on?
            return this.deliveries[trackingCode];
        }

        private static IReadOnlyCollection<DeliverySnapshot> ConvertAggregateToSnapshot(List<Delivery> ret)
        {
            //how do i create snapshots?
            return
                ret.Select(_ => new DeliverySnapshot { Address = _.Address, Exception = _.Exception, Status = _.Status, Parcel = _.Parcel, Recipient = _.Recipient })
                    .ToList();
        }
    }
}