using System.Data.Common;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Agro.Api.Middleware;

/// <summary>
/// Atrapa cualquier excepción no controlada y responde en JSON (ProblemDetails, RFC 9457)
/// con un mensaje claro, sin exponer detalles internos ni credenciales.
/// El detalle completo solo se escribe en el log del servidor.
/// </summary>
public class ManejadorExcepciones(IProblemDetailsService problemDetails, ILogger<ManejadorExcepciones> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (estado, titulo) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Los datos enviados no son válidos."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "La petición no es válida."),
            DbException or TimeoutException => (StatusCodes.Status503ServiceUnavailable,
                "No hay conexión con la base de datos. Intenta de nuevo más tarde."),
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado en el servidor."),
        };

        logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

        var detalle = new ProblemDetails { Status = estado, Title = titulo };
        if (exception is ValidationException validacion)
        {
            // Errores de validación por campo: { "errores": { "correo": ["..."] } }
            detalle.Extensions["errores"] = validacion.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        context.Response.StatusCode = estado;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = detalle,
        });
    }
}
