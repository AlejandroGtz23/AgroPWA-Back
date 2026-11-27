using System;
using System.Collections.Generic;
using System.Text;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class VerificacionCorreoProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IVerificacionCorreoProcedimientos
{
    private const int HorasVigencia = 24;
    private const int MaximoPorHora = 3;

    public async Task<CodigoVerificacionCorreo> CrearCodigoAsync(int idUsuario, string codigoHash, CancellationToken ct = default) =>
        await EjecutarFilaAsync(
            "CALL sp_crear_codigo_verificacion_correo(@id_usuario, @hash, @horas, @maximo, NULL, NULL, NULL)",
            lector => new CodigoVerificacionCorreo(
                lector.GetBoolean(lector.GetOrdinal("p_enviar")),
                lector.IsDBNull(lector.GetOrdinal("p_nombre")) ? null : lector.GetString(lector.GetOrdinal("p_nombre")),
                lector.IsDBNull(lector.GetOrdinal("p_correo")) ? null : lector.GetString(lector.GetOrdinal("p_correo"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("hash", NpgsqlDbType.Varchar, codigoHash),
            Parametro("horas", NpgsqlDbType.Integer, HorasVigencia),
            Parametro("maximo", NpgsqlDbType.Integer, MaximoPorHora));

    public async Task<bool> VerificarAsync(string codigoHash, CancellationToken ct = default) =>
        Convert.ToBoolean(await EjecutarAsync(
            "CALL sp_verificar_correo(@hash, NULL)", ct,
            Parametro("hash", NpgsqlDbType.Varchar, codigoHash)));
}
