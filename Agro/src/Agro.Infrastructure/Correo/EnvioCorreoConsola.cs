using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;


namespace Agro.Infrastructure.Correo;

/// <summary>Desarrollo local sin SMTP: en lugar de mandar el correo, escribe el enlace en la consola de la API.</summary>
public sealed class EnvioCorreoConsola(ConfiguracionCorreo configuracion, ILogger<EnvioCorreoConsola> logger) : IEnvioCorreo
{
    public Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default) =>
        Escribir("Recuperación", correo, PlantillasCorreo.EnlaceConToken(configuracion.UrlRestablecer, token));

    public Task EnviarBienvenidaAsync(string correo, string nombre, string token, CancellationToken ct = default) =>
        Escribir("Bienvenida", correo, PlantillasCorreo.EnlaceConToken(configuracion.UrlVerificarCorreo, token));

    private Task Escribir(string tipo, string correo, string enlace)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning("[Correo local] {Tipo} para {Correo}: {Enlace}", tipo, correo, enlace);
        return Task.CompletedTask;
    }
}
