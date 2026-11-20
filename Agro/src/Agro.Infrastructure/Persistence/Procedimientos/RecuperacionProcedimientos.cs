using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using NpgsqlTypes;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class RecuperacionProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IRecuperacionProcedimientos
{
    private const int MinutosVigencia = 30;
    private const int MaximoPorHora = 3;

    public async Task<CodigoRecuperacion> CrearCodigoAsync(string correo, string codigoHash, CancellationToken ct = default) =>
        await EjecutarFilaAsync(
            "CALL sp_crear_codigo_recuperacion(@correo, @hash, @minutos, @maximo, NULL, NULL)",
            lector => new CodigoRecuperacion(
                lector.GetBoolean(lector.GetOrdinal("p_enviar")),
                lector.IsDBNull(lector.GetOrdinal("p_nombre")) ? null : lector.GetString(lector.GetOrdinal("p_nombre"))),
            ct,
            Parametro("correo", NpgsqlDbType.Varchar, correo),
            Parametro("hash", NpgsqlDbType.Varchar, codigoHash),
            Parametro("minutos", NpgsqlDbType.Integer, MinutosVigencia),
            Parametro("maximo", NpgsqlDbType.Integer, MaximoPorHora));

    public async Task<bool> RestablecerAsync(string codigoHash, string nuevoHash, CancellationToken ct = default) =>
        Convert.ToBoolean(await EjecutarAsync(
            "CALL sp_restablecer_contrasena(@codigo, @hash, NULL)", ct,
            Parametro("codigo", NpgsqlDbType.Varchar, codigoHash),
            Parametro("hash", NpgsqlDbType.Varchar, nuevoHash)));
}