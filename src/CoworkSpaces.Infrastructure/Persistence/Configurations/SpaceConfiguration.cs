using CoworkSpaces.Domain.Entities;
using CoworkSpaces.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoworkSpaces.Infrastructure.Persistence.Configurations;

public class SpaceConfiguration : IEntityTypeConfiguration<Space>
{
    public void Configure(EntityTypeBuilder<Space> builder)
    {
        builder.ToTable("Spaces");

        builder.HasKey(space => space.Id);

        builder.Property(space => space.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(space => space.Capacity)
            .IsRequired();

        builder.Property(space => space.BaseHourlyRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(space => space.OpeningTime)
            .IsRequired();

        builder.Property(space => space.ClosingTime)
            .IsRequired();

        builder.Property(space => space.Status)
            .IsRequired();

        builder.HasMany(space => space.Reservations)
            .WithOne(reservation => reservation.Space)
            .HasForeignKey(reservation => reservation.SpaceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Space
            {
                Id = SeedData.SalaEjecutivaId,
                Name = "Sala Ejecutiva",
                Capacity = 8,
                BaseHourlyRate = 80m,
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(20, 0),
                Status = SpaceStatus.Active
            },
            new Space
            {
                Id = SeedData.SalaDirectorioId,
                Name = "Sala Directorio",
                Capacity = 15,
                BaseHourlyRate = 120m,
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(22, 0),
                Status = SpaceStatus.Active
            },
            new Space
            {
                Id = SeedData.SalaCreativaId,
                Name = "Sala Creativa",
                Capacity = 6,
                BaseHourlyRate = 60m,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(18, 0),
                Status = SpaceStatus.Maintenance
            });
    }
}
