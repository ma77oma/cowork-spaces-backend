using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Application.Mappings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoworkSpaces.Application.Features.Spaces.Queries.GetSpaces;

public class GetSpacesQueryHandler : IRequestHandler<GetSpacesQuery, IReadOnlyCollection<SpaceResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetSpacesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<SpaceResponse>> Handle(GetSpacesQuery request, CancellationToken cancellationToken)
    {
        var spaces = await _context.Spaces
            .AsNoTracking()
            .OrderBy(space => space.Name)
            .ToListAsync(cancellationToken);

        return spaces.Select(space => space.ToResponse()).ToList();
    }
}
