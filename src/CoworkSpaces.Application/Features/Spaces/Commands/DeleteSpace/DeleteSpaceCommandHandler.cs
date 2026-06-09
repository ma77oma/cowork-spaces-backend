using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Spaces.Commands.DeleteSpace;

public class DeleteSpaceCommandHandler : IRequestHandler<DeleteSpaceCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteSpaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await _context.Spaces
            .Include(item => item.Reservations)
            .SingleOrDefaultAsync(space => space.Id == request.Id, cancellationToken);

        if (space is null)
        {
            throw new NotFoundException("Espacio no encontrado.");
        }

        if (space.Reservations.Any())
        {
            throw new BusinessException("No se puede eliminar un espacio que ya tiene reservas asociadas.");
        }

        _context.Spaces.Remove(space);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
