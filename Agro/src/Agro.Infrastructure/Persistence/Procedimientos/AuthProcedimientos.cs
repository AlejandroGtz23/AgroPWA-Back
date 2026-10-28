using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Npgsql;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

/// <summary>Ejecuta los procedimientos almacenados de autenticación.</summary>
public sealed class AuthProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IAuthProcedimientos
{
    // ---------------- Consultas (procedimientos que abren un cursor) ----------------
    public async Task<UsuarioLogin?> ObtenerUsuarioLoginAsync(string correo, CancellationToken ct = default)
    {
        var usuarios = await ConsultarAsync(
            "CALL sp_obtener_usuario_login(@correo, 'resultado')",
            lector => new UsuarioLogin(
                lector.GetInt32(lector.GetOrdinal("id_usuario")),
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetString(lector.GetOrdinal("correo")),
                lector.GetString(lector.GetOrdinal("contrasena_hash")),
                lector.GetString(lector.GetOrdinal("rol")),
                lector.IsDBNull(lector.GetOrdinal("bloqueado_hasta"))
                    ? null
                    : lector.GetFieldValue<DateTime>(lector.GetOrdinal("bloqueado_hasta"))),
            ct,
            Parametro("correo", NpgsqlDbType.Varchar, correo));

        return usuarios.FirstOrDefault();   // null si el correo no existe
    }

    // ---------------- Escrituras ----------------
    public async Task<int> RegistrarUsuarioAsync(NuevoUsuario datos, CancellationToken ct = default)
    {
        try
        {
            var id = await EjecutarAsync(
                "CALL sp_registrar_usuario(@nombre, @paterno, @materno, @correo, @telefono, @nacimiento, @hash, NULL)", ct,
                Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre),
                Parametro("paterno", NpgsqlDbType.Varchar, datos.ApellidoPaterno),
                Parametro("materno", NpgsqlDbType.Varchar, datos.ApellidoMaterno),
                Parametro("correo", NpgsqlDbType.Varchar, datos.Correo),
                Parametro("telefono", NpgsqlDbType.Varchar, datos.Telefono),
                Parametro("nacimiento", NpgsqlDbType.Date, datos.FechaNacimiento),
                Parametro("hash", NpgsqlDbType.Varchar, datos.ContrasenaHash));
            return Convert.ToInt32(id);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictoException("Este correo ya está registrado.");   // 409
        }
    }
    public async Task RegistrarIntentoAccesoAsync(string correo, int? idUsuario, bool exitoso,
        string? ip, string? agenteUsuario, CancellationToken ct = default) =>
        await EjecutarAsync("CALL sp_registrar_intento_acceso(@correo, @id_usuario, @exitoso, @ip, @agente)", ct,
            Parametro("correo", NpgsqlDbType.Varchar, correo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("exitoso", NpgsqlDbType.Boolean, exitoso),
            Parametro("ip", NpgsqlDbType.Varchar, ip),
            Parametro("agente", NpgsqlDbType.Varchar, agenteUsuario));

    public async Task<int> CrearSesionAsync(NuevaSesion sesion, CancellationToken ct = default) =>
        Convert.ToInt32(await EjecutarAsync(
            "CALL sp_crear_sesion(@id_usuario, @jti, @refresh, @expira, @ip, @agente, NULL)", ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, sesion.IdUsuario),
            Parametro("jti", NpgsqlDbType.Varchar, sesion.Jti),
            Parametro("refresh", NpgsqlDbType.Varchar, sesion.RefreshTokenHash),
            Parametro("expira", NpgsqlDbType.TimestampTz, sesion.FechaExpiracion),
            Parametro("ip", NpgsqlDbType.Varchar, sesion.Ip),
            Parametro("agente", NpgsqlDbType.Varchar, sesion.AgenteUsuario)));
    public async Task<bool> ValidarSesionAsync(string jti, int minutosInactividad, CancellationToken ct = default) =>
    Convert.ToBoolean(await EjecutarAsync("CALL sp_validar_sesion(@jti, @minutos, NULL)", ct,
        Parametro("jti", NpgsqlDbType.Varchar, jti),
        Parametro("minutos", NpgsqlDbType.Integer, minutosInactividad)));

    public async Task<UsuarioSesionDto?> RenovarSesionAsync(string refreshTokenHash, string nuevoJti,
        string nuevoRefreshTokenHash, int minutosInactividad, CancellationToken ct = default)
    {
        var usuarios = await ConsultarAsync(
            "CALL sp_renovar_sesion(@refresh, @nuevo_jti, @nuevo_refresh, @minutos, 'resultado')",
            lector => new UsuarioSesionDto(
                lector.GetInt32(lector.GetOrdinal("id_usuario")),
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetString(lector.GetOrdinal("correo")),
                lector.GetString(lector.GetOrdinal("rol"))),
            ct,
            Parametro("refresh", NpgsqlDbType.Varchar, refreshTokenHash),
            Parametro("nuevo_jti", NpgsqlDbType.Varchar, nuevoJti),
            Parametro("nuevo_refresh", NpgsqlDbType.Varchar, nuevoRefreshTokenHash),
            Parametro("minutos", NpgsqlDbType.Integer, minutosInactividad));

        return usuarios.FirstOrDefault();   // vacío = sesión vencida, cerrada o refresh token ya usado
    }

    public async Task<bool> CerrarSesionAsync(string jti, CancellationToken ct = default) =>
    Convert.ToBoolean(await EjecutarAsync("CALL sp_cerrar_sesion(@jti, NULL)", ct,
        Parametro("jti", NpgsqlDbType.Varchar, jti)));
}