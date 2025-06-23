using DispatchManager.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace DispatchManager.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Un error inesperado ha ocurrido");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            message = "Un error ha ocurrido",
            errors = new List<string>()
        };

        switch (exception)
        {
            case NotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    success = false,
                    message = exception.Message,
                    errors = new List<string>()
                };
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    success = false,
                    message = "Falló la validación",
                    errors = new List<string> { validationEx.Message }
                };
                break;

            case BusinessRuleException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    success = false,
                    message = exception.Message,
                    errors = new List<string>()
                };
                break;

            case FluentValidation.ValidationException fluentValidationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    success = false,
                    message = "Validation failed",
                    errors = fluentValidationEx.Errors.Select(e => e.ErrorMessage).ToList()
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    success = false,
                    message = "Ha ocurido un error interno en el servidor",
                    errors = new List<string>()
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
