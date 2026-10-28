using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Parcelas.Common;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class ParcelasProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IParcelasProcedimientos
{
    private const string NombreRepetido = "Ya tienes una parcela con ese nombre.";

    public async Task<ParcelaRegistrada> RegistrarAsync(int idUsuario, Guid? idLocal, DatosParcela datos,
        CancellationToken ct = default)
    {
        try
        {
            return await EjecutarFilaAsync(
                "CALL sp_registrar_parcela(@id_usuario, @id_local, @nombre, @superficie, @ubicacion, NULL, NULL)",
                lector => new ParcelaRegistrada(
                    lector.GetInt32(lector.GetOrdinal("p_id_parcela")),
                    lector.GetBoolean(lector.GetOrdinal("p_ya_existia"))),
                ct,
                Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
                Parametro("id_local", NpgsqlDbType.Uuid, idLocal),
                Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre),
                Parametro("superficie", NpgsqlDbType.Numeric, datos.SuperficieHa),
                Parametro("ubicacion", NpgsqlDbType.Varchar, datos.UbicacionReferencia));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictoException(NombreRepetido);   // 409
        }
    }

    public async Task<bool> EditarAsync(int idParcela, int idUsuario, DatosParcela datos, CancellationToken ct = default)
    {
        try
        {
            return Convert.ToBoolean(await EjecutarAsync(
                "CALL sp_editar_parcela(@id_parcela, @id_usuario, @nombre, @superficie, @ubicacion, NULL)", ct,
                Parametro("id_parcela", NpgsqlDbType.Integer, idParcela),
                Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
                Parametro("nombre", NpgsqlDbType.Varchar, datos.Nombre),
                Parametro("superficie", NpgsqlDbType.Numeric, datos.SuperficieHa),
                Parametro("ubicacion", NpgsqlDbType.Varchar, datos.UbicacionReferencia)
                ));
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictoException(NombreRepetido);   // 409
        }
    }

    public async Task<IReadOnlyList<ParcelaDto>> ObtenerAsync(int idUsuario, int? idParcela
        = null, CancellationToken ct = default) => await ConsultarAsync(
            "CALL sp_obtener_parcelas(@id_usuario, @id_parcela, 'resultado')",
            lector => new ParcelaDto(
                lector.GetInt32(lector.GetOrdinal("id_parcela")),
                lector.IsDBNull(lector.GetOrdinal("id_local"))
                    ? null
                    : lector.GetGuid(lector.GetOrdinal("id_local")),
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetDecimal(lector.GetOrdinal("superficie_ha")),
                lector.IsDBNull(lector.GetOrdinal("ubicacion_referencia"))
                    ? null
                    : lector.GetString(lector.GetOrdinal("ubicacion_referencia")),
                lector.GetInt32(lector.GetOrdinal("numero_cultivos"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("id_parcela", NpgsqlDbType.Integer, idParcela));

}