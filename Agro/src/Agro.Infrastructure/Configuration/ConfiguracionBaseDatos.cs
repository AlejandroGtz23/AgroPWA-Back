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
            Timeout = 5   // si la base no responde en 5 segundos, falla rápido
        };
        return cadena.ConnectionString;
    }
}
