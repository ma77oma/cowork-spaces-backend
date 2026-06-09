using CoworkSpaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Space> Spaces { get; }
    DbSet<Reservation> Reservations { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
