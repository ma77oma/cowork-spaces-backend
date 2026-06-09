using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Commands.DeleteSpace;

public class DeleteSpaceCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}
