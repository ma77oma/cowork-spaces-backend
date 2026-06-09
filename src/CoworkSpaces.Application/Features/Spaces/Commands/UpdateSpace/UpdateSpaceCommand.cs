using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Domain.Enums;
using MediatR;

namespace CoworkSpaces.Application.Features.Spaces.Commands.UpdateSpace;

public class UpdateSpaceCommand : IRequest<SpaceResponse>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal BaseHourlyRate { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public SpaceStatus Status { get; set; }
}
