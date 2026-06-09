using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Domain.Entities;
using CoworkSpaces.Domain.Enums;

namespace CoworkSpaces.Application.Common.Validation;

public static class ReservationBusinessValidator
{
    public static void ValidateReservationWindow(Space space, DateTime startAt, DateTime endAt, DateTime currentTime)
    {
        if (space.Status == SpaceStatus.Maintenance)
        {
            throw new BusinessException("No se puede reservar un espacio en mantenimiento.");
        }

        if (startAt >= endAt)
        {
            throw new BusinessException("La fecha de fin debe ser mayor a la fecha de inicio.");
        }

        if (startAt < currentTime)
        {
            throw new BusinessException("No se puede reservar en el pasado.");
        }

        if (startAt.Date != endAt.Date)
        {
            throw new BusinessException("La reserva debe iniciar y terminar el mismo d\u00eda.");
        }

        var duration = endAt - startAt;
        if (duration < TimeSpan.FromMinutes(30))
        {
            throw new BusinessException("La duraci\u00f3n m\u00ednima de una reserva es de 30 minutos.");
        }

        if (duration > TimeSpan.FromHours(8))
        {
            throw new BusinessException("La duraci\u00f3n m\u00e1xima de una reserva es de 8 horas.");
        }

        var startTime = TimeOnly.FromDateTime(startAt);
        var endTime = TimeOnly.FromDateTime(endAt);

        if (startTime < space.OpeningTime || endTime > space.ClosingTime)
        {
            throw new BusinessException("La reserva debe estar dentro del horario de apertura y cierre del espacio.");
        }
    }

    public static void EnsureNoOverlap(bool existsOverlap)
    {
        if (existsOverlap)
        {
            throw new ConflictException("El espacio ya est\u00e1 reservado en ese horario.");
        }
    }

    public static void ValidateCancellation(Reservation reservation)
    {
        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new BusinessException("La reserva ya fue cancelada.");
        }

        if (reservation.Status == ReservationStatus.Completed)
        {
            throw new BusinessException("Una reserva completada no se puede cancelar.");
        }
    }

    public static void ValidateConfirmation(Reservation reservation)
    {
        if (reservation.Status == ReservationStatus.Confirmed)
        {
            throw new BusinessException("La reserva ya fue confirmada.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new BusinessException("Una reserva cancelada no se puede confirmar.");
        }

        if (reservation.Status == ReservationStatus.Completed)
        {
            throw new BusinessException("Una reserva completada no se puede confirmar.");
        }
    }

    public static void ValidateSpace(Space space)
    {
        if (string.IsNullOrWhiteSpace(space.Name))
        {
            throw new BusinessException("El nombre del espacio es obligatorio.");
        }

        if (space.Name.Length > 150)
        {
            throw new BusinessException("El nombre del espacio no puede superar los 150 caracteres.");
        }

        if (space.Capacity <= 0)
        {
            throw new BusinessException("La capacidad debe ser mayor a cero.");
        }

        if (space.BaseHourlyRate <= 0)
        {
            throw new BusinessException("La tarifa base por hora debe ser mayor a cero.");
        }

        if (space.OpeningTime >= space.ClosingTime)
        {
            throw new BusinessException("La hora de apertura debe ser menor que la hora de cierre.");
        }
    }

    public static void ValidateReportRange(DateTime from, DateTime to)
    {
        if (from >= to)
        {
            throw new BusinessException("El rango del reporte es inv\u00e1lido.");
        }
    }
}
