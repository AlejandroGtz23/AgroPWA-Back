using Agro.Application.Avances.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Agro.Tests.Application.Avances;

/// <summary>Avances en memoria: responde como AvancesProcedimientos ante los errores de sp_registrar_avance.</summary>
internal sealed class AvancesProcedimientosFalsos : IAvancesProcedimientos
{
    private readonly Dictionary<int, (int IdUsuario, bool Activo)> _cultivos = [];
    private readonly List<(int IdUsuario, AvanceDto Avance)> _avances = [];

    public List<TipoActividadDto> Tipos { get; } = [new(1, "Riego"), new(2, "Fertilización"), new(3, "Avance")];
    public NuevoAvance? UltimosDatos { get; private set; }
    public int LlamadasRegistrar { get; private set; }

    public void AgregarCultivo(int idCultivo, int idUsuario, bool activo = true) =>
        _cultivos[idCultivo] = (idUsuario, activo);

    public Task<IReadOnlyList<TipoActividadDto>> ObtenerTiposAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TipoActividadDto>>(Tipos);

    public Task<AvanceRegistrado> RegistrarAsync(int idUsuario, int idCultivo, Guid? idLocal, NuevoAvance datos,
        CancellationToken ct = default)
    {
        LlamadasRegistrar++;
        UltimosDatos = datos;

        var existente = _avances.FirstOrDefault(a =>
            a.IdUsuario == idUsuario && idLocal is not null && a.Avance.IdLocal == idLocal);
        if (existente.Avance is not null)
            return Task.FromResult(new AvanceRegistrado(existente.Avance.Id, YaExistia: true));

        if (!_cultivos.TryGetValue(idCultivo, out var cultivo) || cultivo.IdUsuario != idUsuario)
            throw new NoEncontradoException("Cultivo no encontrado.");                                    // 404
        if (!cultivo.Activo)
            throw new ConflictoException("Solo se pueden registrar avances en cultivos activos.");        // 409
        var tipo = Tipos.FirstOrDefault(t => t.Id == datos.IdTipoActividad)
                   ?? throw new ValidationException([new ValidationFailure("idTipoActividad", "El tipo de actividad no es válido.")]);

        var avance = new AvanceDto(_avances.Count + 1, idLocal, idCultivo, tipo.Id, tipo.Nombre, datos.FechaActividad,
            datos.Descripcion, datos.DuracionMin, datos.MetodoRiego, datos.Producto, datos.Cantidad, datos.Unidad,
            EstadoSincronizacion: "sincronizado");
        _avances.Add((idUsuario, avance));
        return Task.FromResult(new AvanceRegistrado(avance.Id, YaExistia: false));
    }

    public Task<AvanceDto?> ObtenerAsync(int idAvance, int idUsuario, CancellationToken ct = default) =>
        Task.FromResult<AvanceDto?>(
            _avances.FirstOrDefault(a => a.IdUsuario == idUsuario && a.Avance.Id == idAvance).Avance);
}