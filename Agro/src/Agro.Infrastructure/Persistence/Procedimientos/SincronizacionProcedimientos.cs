using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Sincronizacion.Common;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class SincronizacionProcedimientos(AgroDbContext db) : ProcedimientosBase(db), ISincronizacionProcedimientos
{
    public async Task<DispositivoRegistrado> RegistrarDispositivoAsync(int idUsuario, string identificador, string nombre,
        CancellationToken ct = default) =>
        await EjecutarFilaAsync(
            "CALL sp_registrar_dispositivo(@id_usuario, @identificador, @nombre, NULL, NULL)",
            lector => new DispositivoRegistrado(
                lector.GetInt32(lector.GetOrdinal("p_id_dispositivo")),
                lector.GetBoolean(lector.GetOrdinal("p_ya_existia"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("identificador", NpgsqlDbType.Varchar, identificador),
            Parametro("nombre", NpgsqlDbType.Varchar, nombre));

    public async Task<bool> ActualizarPreferenciasAsync(int idDispositivo, int idUsuario, bool sincronizarAuto,
        bool soloWifi, decimal? espacioUsadoMb, CancellationToken ct = default) =>
        Convert.ToBoolean(await EjecutarAsync(
            "CALL sp_actualizar_preferencias_dispositivo(@id_dispositivo, @id_usuario, @auto, @wifi, @espacio, NULL)", ct,
            Parametro("id_dispositivo", NpgsqlDbType.Integer, idDispositivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("auto", NpgsqlDbType.Boolean, sincronizarAuto),
            Parametro("wifi", NpgsqlDbType.Boolean, soloWifi),
            Parametro("espacio", NpgsqlDbType.Numeric, espacioUsadoMb)));

    public async Task<EstadoSincronizacionDto?> ObtenerEstadoAsync(int idDispositivo, int idUsuario,
        CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_estado_sincronizacion(@id_dispositivo, @id_usuario, 'resultado')",
            lector => new EstadoSincronizacionDto(
                lector.GetInt32(lector.GetOrdinal("id_dispositivo")),
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetBoolean(lector.GetOrdinal("sincronizar_auto")),
                lector.GetBoolean(lector.GetOrdinal("solo_wifi")),
                lector.GetDecimal(lector.GetOrdinal("espacio_usado_mb")),
                lector.IsDBNull(lector.GetOrdinal("ultima_sincronizacion"))
                    ? null
                    : lector.GetFieldValue<DateTimeOffset>(lector.GetOrdinal("ultima_sincronizacion")),
                lector.GetInt32(lector.GetOrdinal("errores"))),
            ct,
            Parametro("id_dispositivo", NpgsqlDbType.Integer, idDispositivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

        return filas.Count == 0 ? null : filas[0];
    }
    public async Task RegistrarResultadoAsync(int idUsuario, int idDispositivo, string entidad, Guid idLocal,
    string operacion, string datosJson, string estado, string? mensajeError, CancellationToken ct = default) =>
    await EjecutarAsync(
        "CALL sp_registrar_resultado_sincronizacion(@id_usuario, @id_dispositivo, @entidad, @id_local, @operacion, @datos, @estado, @mensaje)", ct,
        Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
        Parametro("id_dispositivo", NpgsqlDbType.Integer, idDispositivo),
        Parametro("entidad", NpgsqlDbType.Varchar, entidad),
        Parametro("id_local", NpgsqlDbType.Uuid, idLocal),
        Parametro("operacion", NpgsqlDbType.Varchar, operacion),
        Parametro("datos", NpgsqlDbType.Jsonb, datosJson),
        Parametro("estado", NpgsqlDbType.Varchar, estado),
        Parametro("mensaje", NpgsqlDbType.Varchar, mensajeError));

    public async Task MarcarSincronizacionAsync(int idDispositivo, int idUsuario, CancellationToken ct = default) =>
        await EjecutarAsync("CALL sp_marcar_sincronizacion_dispositivo(@id_dispositivo, @id_usuario)", ct,
            Parametro("id_dispositivo", NpgsqlDbType.Integer, idDispositivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

    public async Task<int?> ObtenerIdPorLocalAsync(int idUsuario, string entidad, Guid idLocal, CancellationToken ct = default) =>
        await EjecutarAsync("CALL sp_obtener_id_por_local(@id_usuario, @entidad, @id_local, NULL)", ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("entidad", NpgsqlDbType.Varchar, entidad),
            Parametro("id_local", NpgsqlDbType.Uuid, idLocal)) is int id ? id : null;   // DBNull → no existe
}