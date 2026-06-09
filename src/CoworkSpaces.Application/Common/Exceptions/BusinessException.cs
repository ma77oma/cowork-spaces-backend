namespace CoworkSpaces.Application.Common.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message, object? details = null) : base(message)
    {
        Details = details;
    }

    public object? Details { get; }
}
