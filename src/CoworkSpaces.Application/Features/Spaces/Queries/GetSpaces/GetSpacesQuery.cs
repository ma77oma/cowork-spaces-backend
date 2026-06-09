using CoworkSpaces.Application.DTOs.Spaces;
using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Queries.GetSpaces;

public class GetSpacesQuery : IRequest<IReadOnlyCollection<SpaceResponse>>
{
}
