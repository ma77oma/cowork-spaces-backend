using CoworkSpaces.Application.DTOs.Reports;
using CoworkSpaces.Application.Features.Reports.Queries.GetReservationReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CoworkSpaces.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ReportResponse>> Get([FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetReservationReportQuery { From = from, To = to }, cancellationToken);
        return Ok(response);
    }
}
