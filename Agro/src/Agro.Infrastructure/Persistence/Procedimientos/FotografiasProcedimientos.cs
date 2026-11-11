using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Npgsql;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class FotografiasProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IFotografiasProcedimientos
{
    public async Task<FotografiaRegistrada> RegistrarAsync(int idUsuario, int idCultivo, NuevaFotografia datos,
        CancellationToken ct = default)
    {
        try
        {
            return await EjecutarFilaAsync(
                "CALL sp_registrar_fotografia(@id_usuario, @id_cultivo, @id_avance, @id_local, @ruta, @nombre, @tamano, @portada, @latitud, @longitud, NULL, NULL)",
                lector => new FotografiaRegistrada(
                    lector.GetInt32(lector.GetOrdinal("p_id_fotografia")),
                    lector.GetBoolean(lector.GetOrdinal("p_ya_existia"))),
                ct,
                Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
                Parametro("id_cultivo", NpgsqlDbType.Integer, idCultivo),
                Parametro("id_avance", NpgsqlDbType.Integer, datos.IdAvance),
                Parametro("id_local", NpgsqlDbType.Uuid, datos.IdLocal),
                Parametro("ruta", NpgsqlDbType.Varchar, datos.Ruta),
                Parametro("nombre", NpgsqlDbType.Varchar, datos.NombreArchivo),
                Parametro("tamano", NpgsqlDbType.Integer, datos.TamanoKb),
                Parametro("portada", NpgsqlDbType.Boolean, datos.EsPortada),
                Parametro("latitud", NpgsqlDbType.Numeric, datos.Latitud),
                Parametro("longitud", NpgsqlDbType.Numeric, datos.Longitud));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.NoDataFound && ex.ColumnName is null)
        {
            throw new NoEncontradoException(ex.MessageText);       // 404: el cultivo no existe o no es tuyo
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ObjectNotInPrerequisiteState)
        {
            throw new ConflictoException(ex.MessageText);          // 409: el avance ya tiene 5 fotos
        }
        catch (PostgresException ex) when (ex.ColumnName is not null)
        {
            throw ErrorDeCampo(ex);                                // 400: p. ej. el avance es de otro cultivo
        }
    }

    public async Task<DatosFotografia?> ObtenerAsync(int idFotografia, int idUsuario, CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_fotografia(@id_fotografia, @id_usuario, 'resultado')",
            lector => new DatosFotografia(
                lector.GetInt32(lector.GetOrdinal("id_fotografia")),
                lector.IsDBNull(lector.GetOrdinal("id_local")) ? null : lector.GetGuid(lector.GetOrdinal("id_local")),
                lector.GetInt32(lector.GetOrdinal("id_cultivo")),
                lector.IsDBNull(lector.GetOrdinal("id_avance")) ? null : lector.GetInt32(lector.GetOrdinal("id_avance")),
                lector.GetString(lector.GetOrdinal("archivo_url")),          // es la ruta en el almacenamiento
                lector.GetString(lector.GetOrdinal("nombre_archivo")),
                lector.GetInt32(lector.GetOrdinal("tamano_kb")),
                lector.GetBoolean(lector.GetOrdinal("es_portada")),
                lector.IsDBNull(lector.GetOrdinal("latitud")) ? null : lector.GetDecimal(lector.GetOrdinal("latitud")),
                lector.IsDBNull(lector.GetOrdinal("longitud")) ? null : lector.GetDecimal(lector.GetOrdinal("longitud")),
                lector.GetFieldValue<DateTimeOffset>(lector.GetOrdinal("fecha_captura"))),
            ct,
            Parametro("id_fotografia", NpgsqlDbType.Integer, idFotografia),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

        return filas.Count == 0 ? null : filas[0];
    }

    public async Task<string?> EliminarAsync(int idFotografia, int idUsuario, CancellationToken ct = default) =>
    await EjecutarAsync(
        "CALL sp_eliminar_fotografia(@id_fotografia, @id_usuario, NULL)", ct,
        Parametro("id_fotografia", NpgsqlDbType.Integer, idFotografia),
        Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario)) as string;   // DBNull (no existe) → null
}