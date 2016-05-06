namespace Naos.MessageBus.Persistence
{
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.Its.Domain;

    using Naos.MessageBus.SendingContract;

    public class UpdateTrackedShipment : 
        IUpdateProjectionWhen<Shipment.Created>,
        IUpdateProjectionWhen<Shipment.Rejected>,
        IUpdateProjectionWhen<Shipment.Delivered>,
        IUpdateProjectionWhen<Shipment.Certified>
    {
        private readonly string connectionString;

        public UpdateTrackedShipment(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void UpdateProjection(Shipment.Created @event)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = new ParcelTrackingReport { ParcelId = @event.Parcel.Id, LastEnvelopeId = @event.Parcel.Envelopes.Last().Id };
                dbContext.Shipments.AddOrUpdate(entry);
                dbContext.SaveChanges();
            }
        }

        public void UpdateProjection(Shipment.Rejected @event)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.connectionString))
            {
                // any failure will kill the message sequence
                var entry = dbContext.Shipments.Single(_ => _.ParcelId == @event.AggregateId);
                entry.Status = @event.NewStatus;
                dbContext.SaveChanges();
            }
        }

        public void UpdateProjection(Shipment.Delivered @event)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = dbContext.Shipments.Single(_ => _.ParcelId == @event.AggregateId);

                // if it's the last envelope then update
                if (entry.LastEnvelopeId == @event.TrackingCode.EnvelopeId)
                {
                    entry.Status = @event.NewStatus;
                    dbContext.SaveChanges();
                }
            }
        }

        public void UpdateProjection(Shipment.Certified @event)
        {
            using (var dbContext = new TrackedShipmentDbContext(this.connectionString))
            {
                var entry = new CertifiedNotice
                                {
                                    GroupKey = @event.FilingKey,
                                    Envelope = @event.Envelope,
                                    DeliveredDateUtc = @event.Timestamp.UtcDateTime
                                };

                dbContext.CertifiedNotices.Add(entry);
                dbContext.SaveChanges();
            }
        }
    }
}