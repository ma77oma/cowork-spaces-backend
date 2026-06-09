using CoworkSpaces.Application.DTOs.Reservations;
using CoworkSpaces.Application.Features.Reservations.Commands.CancelReservation;
using CoworkSpaces.Application.Features.Reservations.Commands.ConfirmReservation;
using CoworkSpaces.Application.Features.Reservations.Commands.CreateReservation;
using CoworkSpaces.Application.Features.Reservations.Commands.PreviewReservationPrice;
using CoworkSpaces.Application.Features.Reservations.Queries.GetReservationById;
using CoworkSpaces.Application.Features.Reservations.Queries.GetMyReservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoworkSpaces.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPost("preview-price")]
    public async Task<ActionResult<PreviewPriceResponse>> PreviewPrice([FromBody] PreviewReservationPriceCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CancelReservationResponse>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new CancelReservationCommand { ReservationId = id }, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReservationResponse>> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new ConfirmReservationCommand { ReservationId = id }, cancellationToken);
        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyCollection<ReservationResponse>>> GetMyReservations(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetMyReservationsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetReservationByIdQuery { Id = id }, cancellationToken);
        return Ok(response);
    }
}
