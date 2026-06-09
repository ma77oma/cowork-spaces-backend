using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Domain.Entities;
using CoworkSpaces.Domain.Services;

namespace CoworkSpaces.Application.Mappings;

public static class MappingExtensions
{
    public static SpaceResponse ToResponse(this Space space)
    {
        return new SpaceResponse
        {
            Id = space.Id,
            Name = space.Name,
            Capacity = space.Capacity,
            BaseHourlyRate = space.BaseHourlyRate,
            OpeningTime = space.OpeningTime,
            ClosingTime = space.ClosingTime,
            Status = space.Status
        };
    }

    public static ReservationResponse ToResponse(this Reservation reservation, PricingService pricingService)
    {
        return new ReservationResponse
        {
            Id = reservation.Id,
            SpaceId = reservation.SpaceId,
            SpaceName = reservation.Space?.Name ?? string.Empty,
            StartAt = reservation.StartAt,
            EndAt = reservation.EndAt,
            DurationHours = pricingService.CalculateDurationHours(reservation.StartAt, reservation.EndAt),
            FinalPrice = reservation.FinalPrice,
            RefundAmount = reservation.RefundAmount,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt,
            CancelledAt = reservation.CancelledAt
        };
    }
}
