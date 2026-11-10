using Agro.Application.Avances.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class AvancesProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IAvancesProcedimientos
{
    private const string ZonaHoraria = "America/Mexico_City";

    public async Task<IReadOnlyList<TipoActividadDto>> ObtenerTiposAsync(CancellationToken ct = default) =>
        await ConsultarAsync("CALL sp_obtener_tipos_actividad('resultado')",
            lector => new TipoActividadDto(
                lector.GetInt32(lector.GetOrdinal("id_tipo_actividad")),
                lector.GetString(lector.GetOrdinal("nombre"))),
            ct);


    public async Task<AvanceRegistrado> RegistrarAsync(int idUsuario, int idCultivo, Guid? idLocal, NuevoAvance datos,
        CancellationToken ct = default)
    {
        try
        {
            return await EjecutarFilaAsync(
                "CALL sp_registrar_avance(@id_usuario, @id_cultivo, @id_local, @id_tipo, @fecha, @descripcion, @duracion, @metodo, @producto, @cantidad, @unidad, @zona, NULL, NULL)",
                lector => new AvanceRegistrado(
                    lector.GetInt32(lector.GetOrdinal("p_id_avance")),
                    lector.GetBoolean(lector.GetOrdinal("p_ya_existia"))),
                ct,
                Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
                Parametro("id_cultivo", NpgsqlDbType.Integer, idCultivo),
                Parametro("id_local", NpgsqlDbType.Uuid, idLocal),
                Parametro("id_tipo", NpgsqlDbType.Integer, datos.IdTipoActividad),
                Parametro("fecha", NpgsqlDbType.TimestampTz, datos.FechaActividad.ToUniversalTime()),
                Parametro("descripcion", NpgsqlDbType.Varchar, datos.Descripcion),
                Parametro("duracion", NpgsqlDbType.Integer, datos.DuracionMin),
                Parametro("metodo", NpgsqlDbType.Varchar, datos.MetodoRiego),
                Parametro("producto", NpgsqlDbType.Varchar, datos.Producto),
                Parametro("cantidad", NpgsqlDbType.Numeric, datos.Cantidad),
                Parametro("unidad", NpgsqlDbType.Varchar, datos.Unidad),
                Parametro("zona", NpgsqlDbType.Varchar, ZonaHoraria));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.NoDataFound && ex.ColumnName is null)
        {
            throw new NoEncontradoException(ex.MessageText);       // 404: el cultivo no existe o no es tuyo
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ObjectNotInPrerequisiteState)
        {
            throw new ConflictoException(ex.MessageText);          // 409: el cultivo no está activo
        }
        catch (PostgresException ex) when (ex.ColumnName is not null)
        {
            throw ErrorDeCampo(ex);                                // 400: dato inválido en ese campo
        }
    }

    public async Task<AvanceDto?> ObtenerAsync(int idAvance, int idUsuario, CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_avance(@id_avance, @id_usuario, 'resultado')",
            lector => new AvanceDto(
                lector.GetInt32(lector.GetOrdinal("id_avance")),
                lector.IsDBNull(lector.GetOrdinal("id_local")) ? null : lector.GetGuid(lector.GetOrdinal("id_local")),
                lector.GetInt32(lector.GetOrdinal("id_cultivo")),
                lector.GetInt32(lector.GetOrdinal("id_tipo_actividad")),
                lector.GetString(lector.GetOrdinal("tipo")),
                lector.GetFieldValue<DateTimeOffset>(lector.GetOrdinal("fecha_actividad")),
                lector.GetString(lector.GetOrdinal("descripcion")),
                lector.IsDBNull(lector.GetOrdinal("duracion_min")) ? null : lector.GetInt32(lector.GetOrdinal("duracion_min")),
                lector.IsDBNull(lector.GetOrdinal("metodo_riego")) ? null : lector.GetString(lector.GetOrdinal("metodo_riego")),
                lector.IsDBNull(lector.GetOrdinal("producto")) ? null : lector.GetString(lector.GetOrdinal("producto")),
                lector.IsDBNull(lector.GetOrdinal("cantidad")) ? null : lector.GetDecimal(lector.GetOrdinal("cantidad")),
                lector.IsDBNull(lector.GetOrdinal("unidad")) ? null : lector.GetString(lector.GetOrdinal("unidad")),
                lector.GetString(lector.GetOrdinal("estado_sincronizacion"))),
            ct,
            Parametro("id_avance", NpgsqlDbType.Integer, idAvance),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

        return filas.Count == 0 ? null : filas[0];
    }
}
