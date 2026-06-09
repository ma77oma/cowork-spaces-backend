namespace CoworkSpaces.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message, object? details = null) : base(message)
    {
        Details = details;
    }

    public object? Details { get; }
}
