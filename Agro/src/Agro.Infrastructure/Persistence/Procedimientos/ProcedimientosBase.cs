using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public abstract class ProcedimientosBase(AgroDbContext db)
{
    protected Task<object?> EjecutarAsync(string sql, CancellationToken ct, params NpgsqlParameter[] parametros) =>
        EjecutarComandoAsync(sql, comando => comando.ExecuteScalarAsync(ct), ct, parametros);

    protected Task<T> EjecutarFilaAsync<T>(string sql, Func<NpgsqlDataReader, T> convertir,
        CancellationToken ct, params NpgsqlParameter[] parametros) =>
        EjecutarComandoAsync(sql, async comando =>
        {
            await using var lector = await comando.ExecuteReaderAsync(ct);
            if (!await lector.ReadAsync(ct))
                throw new InvalidOperationException("El procedimiento no devolvió resultados.");
            return convertir(lector);
        }, ct, parametros);

    private async Task<T> EjecutarComandoAsync<T>(string sql, Func<NpgsqlCommand, Task<T>> accion,
        CancellationToken ct, NpgsqlParameter[] parametros)
    {
        var conexion = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametros);
            return await accion(comando);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    protected async Task<List<T>> ConsultarAsync<T>(string sql, Func<NpgsqlDataReader, T> convertir,
        CancellationToken ct, params NpgsqlParameter[] parametros)
    {
        var conexion = (NpgsqlConnection)db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var transaccion = await conexion.BeginTransactionAsync(ct);

            // 1. CALL: el procedimiento abre el cursor
            await using (var llamada = new NpgsqlCommand(sql, conexion, transaccion))
            {
                llamada.Parameters.AddRange(parametros);
                await llamada.ExecuteNonQueryAsync(ct);
            }

            // 2. FETCH: se leen todas las filas del cursor
            var filas = new List<T>();
            await using (var lectura = new NpgsqlCommand("FETCH ALL FROM resultado", conexion, transaccion))
            await using (var lector = await lectura.ExecuteReaderAsync(ct))
            {
                while (await lector.ReadAsync(ct))
                    filas.Add(convertir(lector));
            }

            await transaccion.CommitAsync(ct);   // al terminar la transacción, el cursor se cierra solo
            return filas;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
 protected static NpgsqlParameter Parametro(string nombre, NpgsqlDbType tipo, object? valor) =>
        new(nombre, tipo) { Value = valor ?? DBNull.Value };
}
