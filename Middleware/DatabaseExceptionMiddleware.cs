using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ProductosApi.Middleware;

/// <summary>
/// Atrapa las excepciones que escapan de los controladores y responde con un JSON claro
/// en lugar de tirar la petición. Los errores de MySQL se reportan como 503.
/// </summary>
public class DatabaseExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DatabaseExceptionMiddleware> _logger;

    public DatabaseExceptionMiddleware(RequestDelegate next, ILogger<DatabaseExceptionMiddleware> logger)
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
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "Error de base de datos en {Ruta}", context.Request.Path);
            await EscribirProblema(context, StatusCodes.Status503ServiceUnavailable,
                "Error de base de datos", "No fue posible completar la operación en MySQL. Revisa la conexión.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Ruta}", context.Request.Path);
            await EscribirProblema(context, StatusCodes.Status500InternalServerError,
                "Error interno", "Ocurrió un error inesperado.");
        }
    }

    private static async Task EscribirProblema(HttpContext context, int statusCode, string titulo, string detalle)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = detalle,
            Instance = context.Request.Path
        }, options: null, contentType: "application/problem+json");
    }
}
