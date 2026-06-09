using System.Net;
using System.Text.Json;
using CoworkSpaces.Application.Common.Exceptions;
using CoworkSpaces.Application.Common.Models;

namespace CoworkSpaces.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, details) = exception switch
        {
            ConflictException conflictException => (HttpStatusCode.Conflict, conflictException.Details),
            NotFoundException => (HttpStatusCode.NotFound, null),
            BusinessException businessException => (HttpStatusCode.BadRequest, businessException.Details),
            ValidationException validationException => (HttpStatusCode.BadRequest, (object?)validationException.Errors),
            _ => (HttpStatusCode.InternalServerError, null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = exception.Message,
            Details = details
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
