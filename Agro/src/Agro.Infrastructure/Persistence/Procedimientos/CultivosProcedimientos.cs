using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Results;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class CultivosProcedimientos(AgroDbContext db) : ProcedimientosBase(db), ICultivosProcedimientos
{
    private const string ZonaHoraria = "America/Mexico_City";

    public async Task<CultivoRegistrado> RegistrarAsync(int idUsuario, Guid? idLocal, NuevoCultivo datos,CancellationToken ct = default)
    {
        try
        {
            return await EjecutarFilaAsync(
                "CALL sp_registrar_cultivo(@id_usuario, @id_local, @id_parcela, @id_tipo, @nombre, @variedad, @siembra, @observaciones, NULL, NULL)",
                lector => new CultivoRegistrado(
                    lector.GetInt32(lector.GetOrdinal("p_id_cultivo")),
                    lector.GetBoolean(lector.GetOrdinal("p_ya_existia"))),
                ct,
                Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
                Parametro("id_local", NpgsqlDbType.Uuid, idLocal),
                Parametro("id_parcela", NpgsqlDbType.Integer, datos.IdParcela),
                Parametro("id_tipo", NpgsqlDbType.Integer, datos.IdTipoCultivo),
                Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre),
                Parametro("variedad", NpgsqlDbType.Varchar, datos.Variedad),
                Parametro("siembra", NpgsqlDbType.Date, datos.FechaSiembra),
                Parametro("observaciones", NpgsqlDbType.Varchar, datos.Observaciones));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.NoDataFound)
        {
            // El procedimiento indica la columna: se responde 400 con el error en ese campo
            var campo = ex.ColumnName == "id_parcela" ? "idParcela" : "idTipoCultivo";
            throw new FluentValidation.ValidationException([new ValidationFailure(campo, ex.MessageText)]);
        }
    }
    public async Task<Pagina<CultivoResumenDto>> ObtenerAsync(int idUsuario, FiltroCultivos filtro,
        CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_cultivos(@id_usuario, @buscar, @estado, @id_tipo, @pagina, @tamano, @zona, 'resultado')",
            lector => (Cultivo: new CultivoResumenDto(
                    lector.GetInt32(lector.GetOrdinal("id_cultivo")),
                    lector.IsDBNull(lector.GetOrdinal("id_local")) ? null : lector.GetGuid(lector.GetOrdinal("id_local")),
                    lector.GetString(lector.GetOrdinal("nombre")),
                    lector.GetString(lector.GetOrdinal("tipo")),
                    lector.GetString(lector.GetOrdinal("parcela")),
                    lector.GetFieldValue<DateOnly>(lector.GetOrdinal("fecha_siembra")),
                    lector.GetInt32(lector.GetOrdinal("dias_cultivo")),
                    lector.GetString(lector.GetOrdinal("estado")),
                    lector.GetString(lector.GetOrdinal("estado_sincronizacion")),
                    lector.IsDBNull(lector.GetOrdinal("portada_url")) ? null : lector.GetString(lector.GetOrdinal("portada_url"))),
                Total: lector.GetInt32(lector.GetOrdinal("total"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("buscar", NpgsqlDbType.Varchar, filtro.Buscar),
            Parametro("estado", NpgsqlDbType.Varchar, filtro.Estado),
            Parametro("id_tipo", NpgsqlDbType.Integer, filtro.IdTipoCultivo),
            Parametro("pagina", NpgsqlDbType.Integer, filtro.Pagina),
            Parametro("tamano", NpgsqlDbType.Integer, filtro.Tamano),
            Parametro("zona", NpgsqlDbType.Varchar, ZonaHoraria));

        // Cada fila trae el total sin paginar; sin filas, el total es 0
        return new Pagina<CultivoResumenDto>(filas.Select(f => f.Cultivo).ToList(), filas.Count == 0 ? 0 : filas[0].Total);
    }
    public async Task<IReadOnlyList<TipoCultivoDto>> ObtenerTiposAsync(CancellationToken ct = default) =>
        await ConsultarAsync("CALL sp_obtener_tipos_cultivo('resultado')",
            lector => new TipoCultivoDto(
                lector.GetInt32(lector.GetOrdinal("id_tipo_cultivo")),
                lector.GetString(lector.GetOrdinal("nombre"))),
            ct);

    public async Task<DatosCultivo?> ObtenerDetalleAsync(int idCultivo, int idUsuario, CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_detalle_cultivo(@id_cultivo, @id_usuario, @zona, 'resultado')",
            lector => new DatosCultivo(
                new CultivoDto(
                    lector.GetInt32(lector.GetOrdinal("id_cultivo")),
                    lector.IsDBNull(lector.GetOrdinal("id_local")) ? null : lector.GetGuid(lector.GetOrdinal("id_local")),
                    lector.GetString(lector.GetOrdinal("nombre")),
                    lector.GetInt32(lector.GetOrdinal("id_tipo_cultivo")),
                    lector.GetString(lector.GetOrdinal("tipo")),
                    lector.IsDBNull(lector.GetOrdinal("variedad")) ? null : lector.GetString(lector.GetOrdinal("variedad")),
                    lector.GetInt32(lector.GetOrdinal("id_parcela")),
                    lector.GetString(lector.GetOrdinal("parcela")),
                    lector.GetDecimal(lector.GetOrdinal("superficie_ha")),
                    lector.GetFieldValue<DateOnly>(lector.GetOrdinal("fecha_siembra")),
                    lector.IsDBNull(lector.GetOrdinal("fecha_cosecha"))
                        ? null
                        : lector.GetFieldValue<DateOnly>(lector.GetOrdinal("fecha_cosecha")),
                    lector.GetInt32(lector.GetOrdinal("dias_cultivo")),
                    lector.GetString(lector.GetOrdinal("estado")),
                    lector.GetString(lector.GetOrdinal("estado_sincronizacion")),
                    lector.IsDBNull(lector.GetOrdinal("observaciones")) ? null : lector.GetString(lector.GetOrdinal("observaciones")),
                    lector.IsDBNull(lector.GetOrdinal("portada_url")) ? null : lector.GetString(lector.GetOrdinal("portada_url"))),
                lector.GetInt32(lector.GetOrdinal("total_avances")),
                lector.GetInt32(lector.GetOrdinal("total_fotografias"))),
            ct,
            Parametro("id_cultivo", NpgsqlDbType.Integer, idCultivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("zona", NpgsqlDbType.Varchar, ZonaHoraria));

        return filas.FirstOrDefault();   // null = no existe o es de otro usuario (404)
    }

    public async Task<IReadOnlyList<AvanceResumenDto>> ObtenerAvancesAsync(int idCultivo, int idUsuario,
        CancellationToken ct = default) =>
        await ConsultarAsync("CALL sp_obtener_avances_cultivo(@id_cultivo, @id_usuario, 'resultado')",
            lector => new AvanceResumenDto(
                lector.GetInt32(lector.GetOrdinal("id_avance")),
                lector.GetString(lector.GetOrdinal("tipo")),
                lector.GetFieldValue<DateTime>(lector.GetOrdinal("fecha_actividad")),   // TIMESTAMPTZ → UTC
                lector.GetString(lector.GetOrdinal("descripcion")),
                lector.GetInt32(lector.GetOrdinal("fotos"))),
            ct,
            Parametro("id_cultivo", NpgsqlDbType.Integer, idCultivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));

    public async Task<IReadOnlyList<FotografiaDto>> ObtenerFotografiasAsync(int idCultivo, int idUsuario,
        CancellationToken ct = default) =>
        await ConsultarAsync("CALL sp_obtener_fotografias_cultivo(@id_cultivo, @id_usuario, 'resultado')",
            lector => new FotografiaDto(
                lector.GetInt32(lector.GetOrdinal("id_fotografia")),
                lector.GetString(lector.GetOrdinal("archivo_url")),
                lector.GetFieldValue<DateTime>(lector.GetOrdinal("fecha_captura")),
                lector.GetBoolean(lector.GetOrdinal("es_portada")),
                lector.IsDBNull(lector.GetOrdinal("id_avance")) ? null : lector.GetInt32(lector.GetOrdinal("id_avance"))),
            ct,
            Parametro("id_cultivo", NpgsqlDbType.Integer, idCultivo),
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario));
}
