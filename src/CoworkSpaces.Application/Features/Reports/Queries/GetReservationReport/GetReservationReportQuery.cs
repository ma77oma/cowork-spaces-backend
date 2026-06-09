using CoworkSpaces.Application.DTOs.Reports;
using MediatR;

namespace CoworkSpaces.Application.Features.Reports.Queries.GetReservationReport;

public class GetReservationReportQuery : IRequest<ReportResponse>
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}
