namespace Naos.MessageBus.Persistence
{
    using System.Data.Entity;

    using Naos.MessageBus.DataContract;
    using Naos.MessageBus.SendingContract;

    public class TrackedShipmentDbContext : DbContext
    {
        public TrackedShipmentDbContext(string connectionString) : base(connectionString)
        {
        }

        public virtual DbSet<ParcelTrackingReport> Shipments { get; set; }

        public virtual DbSet<CertifiedNotice> CertifiedNotices { get; set; }

        public virtual DbSet<Envelope> Envelopes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParcelTrackingReport>().HasKey(s => s.ParcelId);
            modelBuilder.Entity<CertifiedNotice>().HasKey(s => s.Id);
        }
    }
}