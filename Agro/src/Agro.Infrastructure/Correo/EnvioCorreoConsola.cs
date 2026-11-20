using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;


namespace Agro.Infrastructure.Correo;

/// <summary>Desarrollo local sin SMTP: en lugar de mandar el correo, escribe el enlace en la consola de la API.</summary>
public sealed class EnvioCorreoConsola(ConfiguracionCorreo configuracion, ILogger<EnvioCorreoConsola> logger) : IEnvioCorreo
{
    public Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("[Correo local] Recuperación para {Correo}: {Enlace}",
                correo, PlantillasCorreo.EnlaceRecuperacion(configuracion.UrlRestablecer, token));
        return Task.CompletedTask;
    }
}
