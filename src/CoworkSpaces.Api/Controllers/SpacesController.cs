using CoworkSpaces.Application.DTOs.Spaces;
using CoworkSpaces.Application.Features.Spaces.Commands.CreateSpace;
using CoworkSpaces.Application.Features.Spaces.Commands.DeleteSpace;
using CoworkSpaces.Application.Features.Spaces.Commands.UpdateSpace;
using CoworkSpaces.Application.Features.Spaces.Queries.GetSpaceById;
using CoworkSpaces.Application.Features.Spaces.Queries.GetSpaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoworkSpaces.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpacesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpacesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SpaceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetSpacesQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpaceResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetSpaceByIdQuery { Id = id }, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<SpaceResponse>> Create([FromBody] CreateSpaceCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SpaceResponse>> Update(Guid id, [FromBody] UpdateSpaceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSpaceCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
