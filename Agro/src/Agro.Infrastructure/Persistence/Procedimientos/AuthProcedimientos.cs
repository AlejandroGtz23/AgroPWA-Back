using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

/// <summary>Ejecuta los procedimientos almacenados de autenticaciónL.</summary>
public sealed class AuthProcedimientos(AgroDbContext db) : IAuthProcedimientos
{
    public async Task<int> RegistrarUsuarioAsync(NuevoUsuario datos, CancellationToken ct = default)
    {
        // Se usa la misma conexión que EF Core (misma cadena del .env)
        var conexion = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var comando = new NpgsqlCommand(
                "CALL sp_registrar_usuario(@nombre, @paterno, @materno, @correo, @telefono, @nacimiento, @hash, NULL)",
                conexion);

            comando.Parameters.Add(Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre));
            comando.Parameters.Add(Parametro("paterno", NpgsqlDbType.Varchar, datos.ApellidoPaterno));
            comando.Parameters.Add(Parametro("materno", NpgsqlDbType.Varchar, datos.ApellidoMaterno));
            comando.Parameters.Add(Parametro("correo", NpgsqlDbType.Varchar, datos.Correo));
            comando.Parameters.Add(Parametro("telefono", NpgsqlDbType.Varchar, datos.Telefono));
            comando.Parameters.Add(Parametro("nacimiento", NpgsqlDbType.Date, datos.FechaNacimiento));
            comando.Parameters.Add(Parametro("hash", NpgsqlDbType.Varchar, datos.ContrasenaHash));

            // CALL devuelve una fila con el parámetro INOUT p_id_usuario
            return Convert.ToInt32(await comando.ExecuteScalarAsync(ct));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Error de la base de datos → excepción de Application (409)
            throw new ConflictoException("Este correo ya está registrado.");
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static NpgsqlParameter Parametro(string nombre, NpgsqlDbType tipo, object? valor) =>
        new(nombre, tipo) { Value = valor ?? DBNull.Value };
}