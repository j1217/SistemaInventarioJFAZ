using Inventory.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (status, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "No encontrado"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado"),
                BusinessRuleException or ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Error no controlado al procesar {Path}", context.Request.Path);

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status < 500 ? exception.Message : "Ocurrió un error inesperado.",
                Instance = context.Request.Path
            });
        }
    }
}
