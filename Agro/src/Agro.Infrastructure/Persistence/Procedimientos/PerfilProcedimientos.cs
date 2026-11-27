using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Perfil.Common;
using NpgsqlTypes;


namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class PerfilProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IPerfilProcedimientos
{
    public async Task<PerfilDto?> ObtenerAsync(int idUsuario, CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_perfil(@id_usuario, 'resultado')",
            lector => new PerfilDto(
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetString(lector.GetOrdinal("apellido_paterno")),
                lector.IsDBNull(lector.GetOrdinal("apellido_materno")) ? null : lector.GetString(lector.GetOrdinal("apellido_materno")),
                lector.GetString(lector.GetOrdinal("correo")),
                lector.IsDBNull(lector.GetOrdinal("telefono")) ? null : lector.GetString(lector.GetOrdinal("telefono")),
                lector.IsDBNull(lector.GetOrdinal("comunidad_municipio")) ? null : lector.GetString(lector.GetOrdinal("comunidad_municipio")),
                lector.GetString(lector.GetOrdinal("rol")),
                lector.GetFieldValue<DateTimeOffset>(lector.GetOrdinal("fecha_registro")),
                lector.GetBoolean(lector.GetOrdinal("correo_verificado"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

        return filas.Count == 0 ? null : filas[0];
    }

    public async Task<bool> ActualizarAsync(int idUsuario, DatosPerfil datos, CancellationToken ct = default) =>
        Convert.ToBoolean(await EjecutarAsync(
            "CALL sp_actualizar_perfil(@id_usuario, @nombre, @paterno, @materno, @telefono, @comunidad, NULL)", ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre),
            Parametro("paterno", NpgsqlDbType.Varchar, datos.ApellidoPaterno),
            Parametro("materno", NpgsqlDbType.Varchar, datos.ApellidoMaterno),
            Parametro("telefono", NpgsqlDbType.Varchar, datos.Telefono),
            Parametro("comunidad", NpgsqlDbType.Varchar, datos.ComunidadMunicipio)));

    public async Task<string?> ObtenerHashAsync(int idUsuario, CancellationToken ct = default) =>
        await EjecutarAsync(
            "CALL sp_obtener_hash_contrasena(@id_usuario, NULL)", ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario)) as string;   // DBNull (no existe) → null

    public async Task<bool> CambiarContrasenaAsync(int idUsuario, string nuevoHash, string jtiActual,
        CancellationToken ct = default) =>
        Convert.ToBoolean(await EjecutarAsync(
            "CALL sp_cambiar_contrasena(@id_usuario, @hash, @jti, NULL)", ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("hash", NpgsqlDbType.Varchar, nuevoHash),
            Parametro("jti", NpgsqlDbType.Varchar, jtiActual)));
}