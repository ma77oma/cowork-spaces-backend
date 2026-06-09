using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Spaces.Queries.GetSpaceById;

public class GetSpaceByIdQueryHandler : IRequestHandler<GetSpaceByIdQuery, SpaceResponse>
{
    private readonly IApplicationDbContext _context;

    public GetSpaceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceResponse> Handle(GetSpaceByIdQuery request, CancellationToken cancellationToken)
    {
        var space = await _context.Spaces
            .AsNoTracking()
            .SingleOrDefaultAsync(space => space.Id == request.Id, cancellationToken);

        if (space is null)
        {
            throw new NotFoundException("Espacio no encontrado.");
        }

        return space.ToResponse();
    }
}
