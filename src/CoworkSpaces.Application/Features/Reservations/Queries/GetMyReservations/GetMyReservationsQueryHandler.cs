using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.Mappings;
using CoworkSpaces.Domain.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandler : IRequestHandler<GetMyReservationsQuery, IReadOnlyCollection<ReservationResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly PricingService _pricingService;

    public GetMyReservationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        PricingService pricingService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pricingService = pricingService;
    }

    public async Task<IReadOnlyCollection<ReservationResponse>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new BusinessException("Debe iniciar sesión para consultar sus reservas.");
        }

        IQueryable<Domain.Entities.Reservation> reservationsQuery = _context.Reservations
            .AsNoTracking()
            .Include(item => item.Space);

        if (!_currentUserService.IsInRole("Admin"))
        {
            reservationsQuery = reservationsQuery.Where(reservation => reservation.CreatedByUserId == userId);
        }

        var reservations = await reservationsQuery
            .OrderByDescending(reservation => reservation.CreatedAt)
            .ToListAsync(cancellationToken);

        return reservations.Select(reservation => reservation.ToResponse(_pricingService)).ToList();
    }
}
