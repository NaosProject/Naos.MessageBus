namespace Naos.MessageBus.Persistence
{
    using System.Data.Entity;

    using Naos.MessageBus.SendingContract;

    public class TrackedShipmentDbContext : DbContext
    {
        public TrackedShipmentDbContext(string connectionString) : base(connectionString)
        {
        }

        public virtual DbSet<TrackedShipment> Shipments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedShipment>().HasKey(s => s.ParcelId);
        }
    }
}