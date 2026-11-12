using Agro.Application.Avances.Commands.RegistrarAvance;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Cultivos.Commands.EditarCultivo;
using Agro.Application.Cultivos.Commands.EliminarCultivo;
using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using Agro.Application.Fotografias.Commands.EliminarFotografia;
using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;
using FluentValidation;
using FluentValidation.Results;
using System.Text.Json;

namespace Agro.Application.Sincronizacion.Common;

/// <summary>
/// Ejecuta una operación de la cola con el mismo handler que se usa en línea (mismas validaciones)
/// y devuelve el id del servidor. Traduce los idLocal de registros creados sin conexión.
/// </summary>
public sealed class EjecutorOperaciones(
    ISincronizacionProcedimientos procedimientos,
    RegistrarParcelaHandler registrarParcela,
    EditarParcelaHandler editarParcela,
    RegistrarCultivoHandler registrarCultivo,
    EditarCultivoHandler editarCultivo,
    EliminarCultivoHandler eliminarCultivo,
    RegistrarAvanceHandler registrarAvance,
    EliminarFotografiaHandler eliminarFotografia)
{
    public const string RegistroNoEncontrado = "El registro no existe en el servidor.";
    public const string DatosInvalidos = "Los datos de la operación no son válidos.";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);   // camelCase, como la API

    public async Task<int> EjecutarAsync(OperacionSincronizacion op, int idUsuario,
        IDictionary<(string Entidad, Guid IdLocal), int> idsServidor, CancellationToken ct = default)
    {
        switch ((op.Entidad, op.Operacion))
        {
            case ("parcela", "crear"):
                {
                    var comando = Leer<RegistrarParcelaCommand>(op) with { IdLocal = op.IdLocal };
                    return (await registrarParcela.EjecutarAsync(idUsuario, comando, ct)).Parcela.Id;
                }
            case ("parcela", "editar"):
                {
                    var id = await IdDestinoAsync(op, idUsuario, idsServidor, ct);
                    return (await editarParcela.EjecutarAsync(id, idUsuario, Leer<EditarParcelaCommand>(op), ct)).Id;
                }
            case ("cultivo", "crear"):
                {
                    var comando = Leer<RegistrarCultivoCommand>(op) with
                    {
                        IdLocal = op.IdLocal,
                        IdParcela = await ReferenciaAsync(op, "parcela", "idParcela", idUsuario, idsServidor, ct)
                    };
                    return (await registrarCultivo.EjecutarAsync(idUsuario, comando, ct)).Cultivo.Id;
                }
            case ("cultivo", "editar"):
                {
                    var id = await IdDestinoAsync(op, idUsuario, idsServidor, ct);
                    var datos = Leer<EditarCultivoCommand>(op);
                    var comando = Leer<EditarCultivoCommand>(op) with
                    {
                        IdParcela = await ReferenciaAsync(op, "parcela", "idParcela", idUsuario, idsServidor, ct)
                    };
                    return (await editarCultivo.EjecutarAsync(id, idUsuario, comando, ct)).Id;
                }
            case ("cultivo", "eliminar"):
                {
                    var id = await IdDestinoAsync(op, idUsuario, idsServidor, ct);
                    await eliminarCultivo.EjecutarAsync(id, idUsuario, ct);
                    return id;
                }
            case ("avance", "crear"):
                {
                    var idCultivo = await ReferenciaAsync(op, "cultivo", "idCultivo", idUsuario, idsServidor, ct)
                                    ?? throw new ValidationException([new ValidationFailure("idCultivo", "El cultivo es obligatorio.")]);
                    var comando = Leer<RegistrarAvanceCommand>(op) with { IdLocal = op.IdLocal };
                    return (await registrarAvance.EjecutarAsync(idCultivo, idUsuario, comando, ct)).Avance.Id;
                }
            case ("fotografia", "eliminar"):
                {
                    var id = await IdDestinoAsync(op, idUsuario, idsServidor, ct);
                    await eliminarFotografia.EjecutarAsync(id, idUsuario, ct);
                    return id;
                }
            default:
                throw new ValidationException([new ValidationFailure("operacion", "La operación no se puede sincronizar.")]);
        }
    }

    /// <summary>Registro que se edita o elimina: "id" en los datos o, si no viene, el de su idLocal.</summary>
    private async Task<int> IdDestinoAsync(OperacionSincronizacion op, int idUsuario,
        IDictionary<(string Entidad, Guid IdLocal), int> idsServidor, CancellationToken ct) =>
        Entero(op.Datos, "id")
        ?? await TraducirAsync(op.Entidad, op.IdLocal!.Value, idUsuario, idsServidor, ct)
        ?? throw new NoEncontradoException(RegistroNoEncontrado);

    /// <summary>
    /// Referencia a otro registro: su id ("idParcela") o el idLocal de uno creado sin conexión ("idLocalParcela").
    /// </summary>
    private async Task<int?> ReferenciaAsync(OperacionSincronizacion op, string entidad, string campo, int idUsuario,
        IDictionary<(string Entidad, Guid IdLocal), int> idsServidor, CancellationToken ct)
    {
        if (Entero(op.Datos, campo) is int id)
            return id;

        var campoLocal = $"idLocal{char.ToUpperInvariant(entidad[0])}{entidad[1..]}";   // idLocalParcela, idLocalCultivo
        if (op.Datos.ValueKind != JsonValueKind.Object
            || !op.Datos.TryGetProperty(campoLocal, out var valor)
            || !valor.TryGetGuid(out var idLocal))
            return null;   // sin referencia: el validador del handler dirá que es obligatoria

        return await TraducirAsync(entidad, idLocal, idUsuario, idsServidor, ct)
              ?? throw new ValidationException([new ValidationFailure(campo, $"El registro relacionado ({entidad}) todavía no está sincronizado.")]);
    }

    /// <summary>idLocal → id: primero lo creado en este mismo lote y, si no, lo que ya está en la BD.</summary>
    private async Task<int?> TraducirAsync(string entidad, Guid idLocal, int idUsuario,
        IDictionary<(string Entidad, Guid IdLocal), int> idsServidor, CancellationToken ct) =>
        idsServidor.TryGetValue((entidad, idLocal), out var id)
            ? id
            : await procedimientos.ObtenerIdPorLocalAsync(idUsuario, entidad, idLocal, ct);

    private static int? Entero(JsonElement datos, string campo) =>
        datos.ValueKind == JsonValueKind.Object && datos.TryGetProperty(campo, out var valor)
        && valor.ValueKind == JsonValueKind.Number && valor.TryGetInt32(out var numero)
            ? numero
            : null;

    private static T Leer<T>(OperacionSincronizacion op) =>
        op.Datos.ValueKind == JsonValueKind.Object
            ? op.Datos.Deserialize<T>(Json)!
            : throw new ValidationException([new ValidationFailure("datos", DatosInvalidos)]);
}