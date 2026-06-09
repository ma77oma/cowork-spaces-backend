using FluentValidation;

namespace CoworkSpaces.Application.Features.Reports.Queries.GetReservationReport;

public class GetReservationReportQueryValidator : AbstractValidator<GetReservationReportQuery>
{
    public GetReservationReportQueryValidator()
    {
        RuleFor(query => query.From)
            .NotEmpty().WithMessage("La fecha desde es obligatoria.");

        RuleFor(query => query.To)
            .NotEmpty().WithMessage("La fecha hasta es obligatoria.");

        RuleFor(query => query)
            .Must(query => query.From <= query.To)
            .WithMessage("La fecha desde debe ser menor o igual que la fecha hasta.");
    }
}
