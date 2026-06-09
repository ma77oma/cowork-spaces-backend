using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Domain.Enums;
using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Commands.CreateSpace;

public class CreateSpaceCommand : IRequest<SpaceResponse>
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public SpaceStatus Status { get; set; }
}
