using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;
using Agro.Application.Sincronizacion.Common;
using FluentValidation;
using System.Text.Json;

namespace Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;

public sealed class ProcesarSincronizacionHandler(
    ISincronizacionProcedimientos procedimientos,
    EjecutorOperaciones ejecutor,
    IValidator<ProcesarSincronizacionCommand> validador)
{
    public const string Sincronizado = "sincronizado";
    public const string Error = "error";

    // Si dos operaciones tienen la misma fecha: primero lo que otras necesitan
    private static readonly string[] OrdenEntidades = ["parcela", "cultivo", "avance", "fotografia"];

    public async Task<ResultadoSincronizacionDto> EjecutarAsync(int idUsuario, ProcesarSincronizacionCommand comando,
        CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                       // 400

        var idDispositivo = comando.IdDispositivo!.Value;
        _ = await procedimientos.ObtenerEstadoAsync(idDispositivo, idUsuario, ct)
            ?? throw new NoEncontradoException(ActualizarPreferenciasHandler.NoEncontrado);       // 404

        var idsServidor = new Dictionary<(string Entidad, Guid IdLocal), int>();
        var resultados = new List<ResultadoOperacionDto>();

        // En el orden en que se crearon en el dispositivo
        var enOrden = comando.Operaciones
            .OrderBy(o => o.FechaCreacion ?? DateTimeOffset.MinValue)
            .ThenBy(o => Array.IndexOf(OrdenEntidades, o.Entidad));

        foreach (var operacion in enOrden)
        {
            var resultado = await ProcesarAsync(operacion, idUsuario, idsServidor, ct);
            resultados.Add(resultado);

            await procedimientos.RegistrarResultadoAsync(idUsuario, idDispositivo, operacion.Entidad,
                operacion.IdLocal!.Value, operacion.Operacion,
                operacion.Datos.ValueKind == JsonValueKind.Object ? operacion.Datos.GetRawText() : "{}",
                resultado.Estado, resultado.MensajeError, ct);
        }

        await procedimientos.MarcarSincronizacionAsync(idDispositivo, idUsuario, ct);

        return new ResultadoSincronizacionDto(
            resultados.Count(r => r.Estado == Sincronizado), resultados.Count(r => r.Estado == Error), resultados);
    }

    /// <summary>Una operación que falla se anota como error y no detiene las demás.</summary>
    private async Task<ResultadoOperacionDto> ProcesarAsync(OperacionSincronizacion operacion, int idUsuario,
        Dictionary<(string Entidad, Guid IdLocal), int> idsServidor, CancellationToken ct)
    {
        var idLocal = operacion.IdLocal!.Value;
        try
        {
            var id = await ejecutor.EjecutarAsync(operacion, idUsuario, idsServidor, ct);
            idsServidor[(operacion.Entidad, idLocal)] = id;
            return new ResultadoOperacionDto(idLocal, operacion.Entidad, operacion.Operacion, Sincronizado, id, null);
        }
        catch (Exception ex) when (ex is ValidationException or NoEncontradoException or ConflictoException or JsonException)
        {
            return new ResultadoOperacionDto(idLocal, operacion.Entidad, operacion.Operacion, Error, null, MensajeDe(ex));
        }
    }

    private static string MensajeDe(Exception ex) => ex switch
    {
        ValidationException { Errors: var errores } when errores.Any() =>
            string.Join(" ", errores.Select(e => e.ErrorMessage).Distinct()),
        JsonException => EjecutorOperaciones.DatosInvalidos,
        _ => ex.Message
    };
}