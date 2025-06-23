using DispatchManager.Domain.Entities;
using DispatchManager.Domain.ValueObjects;
using DispatchManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatchManager.Infrastructure.Configuration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // Primary Key
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedNever(); // Controlled by domain

        // Foreign Keys
        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.ProductId)
            .IsRequired();

        // Value Objects - Quantity
        builder.OwnsOne(o => o.Quantity, quantity =>
        {
            quantity.Property(q => q.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        // Value Objects - Origin Coordinate
        builder.OwnsOne(o => o.Origin, origin =>
        {
            origin.Property(c => c.Latitude)
                .HasColumnName("OriginLatitude")
                .HasColumnType("decimal(10,8)")
                .IsRequired();

            origin.Property(c => c.Longitude)
                .HasColumnName("OriginLongitude")
                .HasColumnType("decimal(11,8)")
                .IsRequired();
        });

        // Value Objects - Destination Coordinate
        builder.OwnsOne(o => o.Destination, destination =>
        {
            destination.Property(c => c.Latitude)
                .HasColumnName("DestinationLatitude")
                .HasColumnType("decimal(10,8)")
                .IsRequired();

            destination.Property(c => c.Longitude)
                .HasColumnName("DestinationLongitude")
                .HasColumnType("decimal(11,8)")
                .IsRequired();
        });

        // Value Objects - Distance
        builder.OwnsOne(o => o.Distance, distance =>
        {
            distance.Property(d => d.Kilometers)
                .HasColumnName("DistanceKm")
                .HasColumnType("decimal(8,2)")
                .IsRequired();
        });

        // Value Objects - DeliveryCost
        builder.OwnsOne(o => o.Cost, cost =>
        {
            cost.Property(c => c.Amount)
                .HasColumnName("CostAmount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            cost.Property(c => c.Currency)
                .HasColumnName("CostCurrency")
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");
        });

        // Enum
        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        // Audit Fields
        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired(false);

        // Navigation Properties
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Product)
            .WithMany()
            .HasForeignKey(o => o.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("IX_Orders_CustomerId");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        builder.HasIndex(o => new { o.CustomerId, o.Status })
            .HasDatabaseName("IX_Orders_CustomerId_Status");

        // Ignore Domain Events (not persisted)
        builder.Ignore(o => o.DomainEvents);
    }
}