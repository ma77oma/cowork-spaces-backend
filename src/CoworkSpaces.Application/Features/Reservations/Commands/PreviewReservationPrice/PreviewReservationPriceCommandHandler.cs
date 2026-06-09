using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Commands.PreviewReservationPrice;

public class PreviewReservationPriceCommandHandler : IRequestHandler<PreviewReservationPriceCommand, PreviewPriceResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ISystemClock _clock;
    private readonly PricingService _pricingService;

    public PreviewReservationPriceCommandHandler(
        IApplicationDbContext context,
        ISystemClock clock,
        PricingService pricingService)
    {
        _context = context;
        _clock = clock;
        _pricingService = pricingService;
    }

    public async Task<PreviewPriceResponse> Handle(PreviewReservationPriceCommand request, CancellationToken cancellationToken)
    {
        var space = await _context.Spaces
            .AsNoTracking()
            .SingleOrDefaultAsync(space => space.Id == request.SpaceId, cancellationToken);

        if (space is null)
        {
            throw new NotFoundException("Espacio no encontrado.");
        }

        ReservationBusinessValidator.ValidateReservationWindow(space, request.StartAt, request.EndAt, _clock.Now);

        return new PreviewPriceResponse
        {
            SpaceId = space.Id,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            DurationHours = _pricingService.CalculateDurationHours(request.StartAt, request.EndAt),
            BaseHourlyRate = space.BaseHourlyRate,
            FinalPrice = _pricingService.CalculateFinalPrice(space.BaseHourlyRate, request.StartAt, request.EndAt, _clock.Now)
        };
    }
}
