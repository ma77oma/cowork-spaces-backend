using CoworkSpaces.Application.Common.Interfaces;
using CoworkSpaces.Application.Common.Validation;
using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Application.Mappings;
using CoworkSpaces.Domain.Entities;
using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Commands.CreateSpace;

public class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, SpaceResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateSpaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceResponse> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        var space = new Space
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            BaseHourlyRate = request.BaseHourlyRate,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime,
            Status = request.Status
        };

        ReservationBusinessValidator.ValidateSpace(space);

        await _context.Spaces.AddAsync(space, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return space.ToResponse();
    }
}
