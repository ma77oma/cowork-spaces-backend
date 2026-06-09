using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.Mappings;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Queries.GetReservationById;

public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly PricingService _pricingService;

    public GetReservationByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, PricingService pricingService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pricingService = pricingService;
    }

    public async Task<ReservationResponse> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
    {
        var reservation = await _context.Reservations
            .AsNoTracking()
            .Include(item => item.Space)
            .SingleOrDefaultAsync(reservation => reservation.Id == request.Id, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reserva no encontrada.");
        }

        if (reservation.CreatedByUserId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin"))
        {
            throw new BusinessException("No tiene permisos para consultar esta reserva.");
        }

        return reservation.ToResponse(_pricingService);
    }
}
