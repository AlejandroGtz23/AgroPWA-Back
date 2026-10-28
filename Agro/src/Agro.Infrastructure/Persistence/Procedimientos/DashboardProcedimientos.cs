using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Dashboard.Common;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class DashboardProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IDashboardProcedimientos
{
    private const string ZonaHoraria = "America/Mexico_City";

    public async Task<TotalesDashboard?> ObtenerTotalesAsync(int idUsuario, CancellationToken ct = default)
    {
        var filas = await ConsultarAsync(
            "CALL sp_obtener_resumen_dashboard(@id_usuario, @zona, 'resultado')",
            lector => new TotalesDashboard(
                lector.GetString(lector.GetOrdinal("nombre")),
                lector.GetInt32(lector.GetOrdinal("cultivos_activos")),
                lector.GetInt32(lector.GetOrdinal("avances_del_mes")),
                lector.GetBoolean(lector.GetOrdinal("tiene_parcela")),
                lector.GetBoolean(lector.GetOrdinal("tiene_cultivo")),
                lector.GetBoolean(lector.GetOrdinal("tiene_avance"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("zona", NpgsqlDbType.Varchar, ZonaHoraria));

        return filas.FirstOrDefault();   // sin fila = usuario inexistente o desactivado
    }

    public async Task<IReadOnlyList<ActividadRecienteDto>> ObtenerActividadRecienteAsync(int idUsuario, int limite, CancellationToken ct = default) =>
        await ConsultarAsync(
            "CALL sp_obtener_actividad_reciente(@id_usuario, @limite, 'resultado')",
            lector => new ActividadRecienteDto(
                lector.GetString(lector.GetOrdinal("tipo")),
                lector.GetString(lector.GetOrdinal("cultivo")),
                lector.GetFieldValue<DateTime>(lector.GetOrdinal("fecha"))),
            ct,
            Parametro("id_usuario", NpgsqlDbType.Integer, idUsuario),
            Parametro("limite", NpgsqlDbType.Integer, limite));
}

