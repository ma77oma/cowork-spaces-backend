using CoworkSpaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoworkSpaces.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.SpaceId)
            .IsRequired();

        builder.Property(reservation => reservation.CreatedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(reservation => reservation.CancelledByUserId)
            .HasMaxLength(450);

        builder.Property(reservation => reservation.StartAt)
            .IsRequired();

        builder.Property(reservation => reservation.EndAt)
            .IsRequired();

        builder.Property(reservation => reservation.Status)
            .IsRequired();

        builder.Property(reservation => reservation.FinalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(reservation => reservation.RefundAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(reservation => reservation.CreatedAt)
            .IsRequired();

        builder.Property(reservation => reservation.CancelledAt);

        builder.HasIndex(reservation => new { reservation.SpaceId, reservation.StartAt, reservation.EndAt, reservation.Status })
            .HasDatabaseName("IX_Reservations_SpaceId_StartAt_EndAt_Status");

        builder.HasIndex(reservation => new { reservation.StartAt, reservation.EndAt })
            .HasDatabaseName("IX_Reservations_StartAt_EndAt");
    }
}
