﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackedShipmentDbContext.cs" company="Naos">
//    Copyright (c) Naos 2017. All Rights Reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Naos.MessageBus.Persistence
{
    using System.Data.Entity;

    using Naos.MessageBus.Domain;

    using Spritely.Recipes;

    /// <summary>
    /// Database context for the read model tracking shipments.
    /// </summary>
    public class TrackedShipmentDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackedShipmentDbContext"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to the database.</param>
        public TrackedShipmentDbContext(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Gets or sets the shipments.
        /// </summary>
        public virtual DbSet<ShipmentForDatabase> Shipments { get; set; }

        /// <summary>
        /// Gets or sets the notices.
        /// </summary>
        public virtual DbSet<NoticeForDatabase> Notices { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            new { modelBuilder }.Must().NotBeNull().OrThrowFirstFailure();

            modelBuilder.Entity<ShipmentForDatabase>().HasKey(s => s.ParcelId);
            modelBuilder.Entity<NoticeForDatabase>().HasKey(s => s.Id);
        }
    }
}