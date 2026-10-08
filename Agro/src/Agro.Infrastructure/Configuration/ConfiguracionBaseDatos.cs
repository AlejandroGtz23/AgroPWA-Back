using Npgsql;

namespace Agro.Infrastructure.Configuration;

/// <summary>
/// Arma la cadena de conexión a PostgreSQL con las variables de entorno DB_* (archivo .env).
/// Nunca se escriben credenciales en el código.
/// </summary>
public static class ConfiguracionBaseDatos
{
    public static string CadenaConexion()
    {
        var cadena = new NpgsqlConnectionStringBuilder
        {
            Host = VariablesEntorno.Obtener("DB_HOST"),
            Port = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var puerto) ? puerto : 5432,
            Database = VariablesEntorno.Obtener("DB_NAME"),
            Username = VariablesEntorno.Obtener("DB_USER"),
            Password = VariablesEntorno.Obtener("DB_PASSWORD"),
            SslMode = ModoSsl(),
            Timeout = 15   // si la base no responde en 5 segundos, falla rápido
        };
        return cadena.ConnectionString; 
    }

    /// <summary>
    /// DB_SSLMODE: Disable, Prefer, Require, VerifyCA o VerifyFull.
    /// Azure exige "Require". Si no se indica, se usa "Prefer" (sirve para el contenedor local).
    /// </summary>
    /// 
    private static SslMode ModoSsl()
    {
        var valor = Environment.GetEnvironmentVariable("DB_SSLMODE");
        if (string.IsNullOrWhiteSpace(valor))
            return SslMode.Prefer;

        return Enum.TryParse<SslMode>(valor, ignoreCase: true, out var modo)
            ? modo
            : throw new InvalidOperationException(
                $"DB_SSLMODE tiene un valor no válido: '{valor}'. Usa Disable, Prefer, Require, VerifyCA o VerifyFull.");
    }
}
