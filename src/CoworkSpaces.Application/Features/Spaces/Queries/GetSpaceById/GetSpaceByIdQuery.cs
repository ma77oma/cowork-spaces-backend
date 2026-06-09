using CoworkSpaces.Application.DTOs.Spaces;
using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Queries.GetSpaceById;

public class GetSpaceByIdQuery : IRequest<SpaceResponse>
{
    public Guid Id { get; set; }
}
