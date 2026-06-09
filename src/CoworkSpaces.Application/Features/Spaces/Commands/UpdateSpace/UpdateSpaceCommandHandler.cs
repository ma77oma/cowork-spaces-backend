using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Spaces.Commands.UpdateSpace;

public class UpdateSpaceCommandHandler : IRequestHandler<UpdateSpaceCommand, SpaceResponse>
{
    private readonly IApplicationDbContext _context;

    public UpdateSpaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceResponse> Handle(UpdateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = await _context.Spaces.SingleOrDefaultAsync(space => space.Id == request.Id, cancellationToken);

        if (space is null)
        {
            throw new NotFoundException("Espacio no encontrado.");
        }

        space.Name = request.Name.Trim();
        space.Capacity = request.Capacity;
        space.BaseHourlyRate = request.BaseHourlyRate;
        space.OpeningTime = request.OpeningTime;
        space.ClosingTime = request.ClosingTime;
        space.Status = request.Status;

        ReservationBusinessValidator.ValidateSpace(space);

        await _context.SaveChangesAsync(cancellationToken);
        return space.ToResponse();
    }
}
