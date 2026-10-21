using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Agro.Infrastructure.Configuration;
/// <summary>
/// Datos para validar y firmar y validar los jwt leidos del .env
/// </summary>
public sealed record ConfiguracionJwt(string Clave, string Emisor, string Audiencia, int MinutosAcceso)
{
    public static ConfiguracionJwt DesdeVariables()
    {
        var clave = VariablesEntorno.Obtener("JWT_CLAVE");
        if (Encoding.UTF8.GetByteCount(clave) < 32)
            throw new InvalidOperationException("JWT_CLAVE debe de tener al menos 32 caracteres");
        return new ConfiguracionJwt(
            clave,
             Opcional("JWT_EMISOR", "agro_api"),
             Opcional("JWT_AUDIENCIA", "agro_pwa"),
             int.TryParse(Environment.GetEnvironmentVariable("JWT_MINUTOS"), out var minutos) ? minutos : 20);
    }

    private static string Opcional(string nombre, string porDefecto) => 
        Environment.GetEnvironmentVariable(nombre) is { Length: > 0 } valor ? valor : porDefecto;
}
