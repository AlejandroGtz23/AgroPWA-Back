using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Agro.Infrastructure.Persistence;

/// <summary>
/// Prueba la conexión a PostgreSQL al arrancar la API (criterios 2 y 4 de la HU-04).
/// Devuelve un mensaje claro del motivo SIN incluir la contraseña ni la cadena de conexión.
/// </summary>
public static class VerificacionConexion
{
    public static async Task<(bool Conectado, string Mensaje)> ProbarAsync(AgroDbContext db, CancellationToken ct = default)
    {
        var cadena = new NpgsqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        var destino = $"{cadena.Database} en {cadena.Host}:{cadena.Port}";
        try
        {
            await db.Database.OpenConnectionAsync(ct);
            await db.Database.CloseConnectionAsync();
            return (true, $"Conexión exitosa a la base de datos {destino}");
        }
        catch (Exception ex) when (BuscarError(ex) is not null)
        {
            var motivo = BuscarError(ex) switch
            {
                PostgresException { SqlState: "28P01" } => "usuario o contraseña incorrectos; revisa DB_USER y DB_PASSWORD en el .env",
                PostgresException { SqlState: "3D000" } => $"la base de datos {cadena.Database} no existe; ejecuta los scripts de la HU-03",
                PostgresException pg => $"PostgreSQL respondió con el código {pg.SqlState}",
                _ => $"no se pudo llegar a {cadena.Host}:{cadena.Port}; revisa que el contenedor de PostgreSQL esté encendido",
            };
            return (false, $"No se pudo conectar a la base de datos {destino}: {motivo}");
        }
    }

    /// <summary>
    /// EF Core puede envolver el error de Npgsql en otra excepción ("transient failure"),
    /// así que se busca en toda la cadena de InnerException.
    /// </summary>
    private static Exception? BuscarError(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
        {
            if (ex is NpgsqlException or TimeoutException) return ex;
        }
        return null;
    }
}
