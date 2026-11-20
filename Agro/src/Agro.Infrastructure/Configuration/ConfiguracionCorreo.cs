using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Configuration;

/// <summary>
/// SMTP_* del .env. Si falta SMTP_HOST, los correos se escriben en la consola (desarrollo local).
/// </summary>
public sealed record ConfiguracionCorreo(string? Host, int Puerto, string? Usuario, string? Contrasena,
    string Remitente, string UrlRestablecer)
{
    public bool UsaSmtp => !string.IsNullOrWhiteSpace(Host);

    public static ConfiguracionCorreo DesdeVariables()
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST");
        var usuario = string.IsNullOrWhiteSpace(host) ? null : VariablesEntorno.Obtener("SMTP_USUARIO");

        return new ConfiguracionCorreo(
            host,
            int.TryParse(Environment.GetEnvironmentVariable("SMTP_PUERTO"), out var puerto) ? puerto : 587,
            usuario,
            // Con SMTP la contraseña es obligatoria: si falta, la API avisa y no arranca
            string.IsNullOrWhiteSpace(host) ? null : VariablesEntorno.Obtener("SMTP_CONTRASENA"),
            Environment.GetEnvironmentVariable("SMTP_REMITENTE") is { Length: > 0 } remitente ? remitente : usuario ?? "agro@localhost",
            Environment.GetEnvironmentVariable("URL_RESTABLECER") is { Length: > 0 } url ? url : "http://localhost:5173/restablecer");
    }
}