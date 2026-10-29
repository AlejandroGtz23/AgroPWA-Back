using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using FluentValidation;
using FluentValidation.Results;

namespace Agro.Tests.Application.Cultivos;

/// <summary>
/// Simula los procedimientos de cultivos (migración 013) en memoria, con las mismas reglas:
/// idLocal repetido devuelve el existente, la parcela debe ser del usuario, el tipo debe existir
/// y cada usuario solo ve sus cultivos.
/// </summary>
internal sealed class CultivosProcedimientosFalsos : ICultivosProcedimientos
{
    private readonly Dictionary<int, int> _duenoParcela = [];          // idParcela → idUsuario
    private readonly List<(int IdUsuario, CultivoDto Cultivo)> _cultivos = [];

    public List<TipoCultivoDto> Tipos { get; } = [new(1, "Maíz"), new(2, "Frijol"), new(3, "Calabaza")];
    public List<AvanceResumenDto> Avances { get; } = [];
    public List<FotografiaDto> Fotografias { get; } = [];

    public int LlamadasRegistrar { get; private set; }
    public int LlamadasAvances { get; private set; }
    public FiltroCultivos? UltimoFiltro { get; private set; }
    public NuevoCultivo? UltimosDatos { get; private set; }

    public void AgregarParcela(int idParcela, int idUsuario) => _duenoParcela[idParcela] = idUsuario;

    public Task<IReadOnlyList<TipoCultivoDto>> ObtenerTiposAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TipoCultivoDto>>(Tipos);

    public Task<CultivoRegistrado> RegistrarAsync(int idUsuario, Guid? idLocal, NuevoCultivo datos,
        CancellationToken ct = default)
    {
        LlamadasRegistrar++;
        UltimosDatos = datos;

        var existente = _cultivos.FirstOrDefault(c =>
            c.IdUsuario == idUsuario && idLocal is not null && c.Cultivo.IdLocal == idLocal);
        if (existente.Cultivo is not null)
            return Task.FromResult(new CultivoRegistrado(existente.Cultivo.Id, YaExistia: true));

        // Igual que CultivosProcedimientos cuando el procedimiento lanza P0002
        if (!_duenoParcela.TryGetValue(datos.IdParcela, out var dueno) || dueno != idUsuario)
            throw new ValidationException([new ValidationFailure("idParcela", "La parcela no existe.")]);
        var tipo = Tipos.FirstOrDefault(t => t.Id == datos.IdTipoCultivo)
                   ?? throw new ValidationException([new ValidationFailure("idTipoCultivo", "El tipo de cultivo no es válido.")]);

        var cultivo = new CultivoDto(
            _cultivos.Count + 1, idLocal, datos.Nombre, tipo.Id, tipo.Nombre, datos.Variedad,
            datos.IdParcela, $"Parcela {datos.IdParcela}", 1.5m, datos.FechaSiembra, FechaCosecha: null,
            DiasCultivo: 10, Estado: "activo", EstadoSincronizacion: "sincronizado", datos.Observaciones, PortadaUrl: null);
        _cultivos.Add((idUsuario, cultivo));
        return Task.FromResult(new CultivoRegistrado(cultivo.Id, YaExistia: false));
    }

    public Task<Pagina<CultivoResumenDto>> ObtenerAsync(int idUsuario, FiltroCultivos filtro, CancellationToken ct = default)
    {
        UltimoFiltro = filtro;
        var propios = _cultivos
            .Where(c => c.IdUsuario == idUsuario && (filtro.Estado is null || c.Cultivo.Estado == filtro.Estado))
            .Select(c => c.Cultivo)
            .Reverse()   // del más reciente al más antiguo
            .Select(c => new CultivoResumenDto(c.Id, c.IdLocal, c.Nombre, c.Tipo, c.Parcela, c.FechaSiembra,
                c.DiasCultivo, c.Estado, c.EstadoSincronizacion, c.PortadaUrl))
            .ToList();
        var pagina = propios.Skip((filtro.Pagina - 1) * filtro.Tamano).Take(filtro.Tamano).ToList();
        return Task.FromResult(new Pagina<CultivoResumenDto>(pagina, propios.Count));
    }

    public Task<DatosCultivo?> ObtenerDetalleAsync(int idCultivo, int idUsuario, CancellationToken ct = default)
    {
        var cultivo = _cultivos.FirstOrDefault(c => c.IdUsuario == idUsuario && c.Cultivo.Id == idCultivo).Cultivo;
        return Task.FromResult(cultivo is null ? null : new DatosCultivo(cultivo, Avances.Count, Fotografias.Count));
    }

    public Task<IReadOnlyList<AvanceResumenDto>> ObtenerAvancesAsync(int idCultivo, int idUsuario,
        CancellationToken ct = default)
    {
        LlamadasAvances++;
        return Task.FromResult<IReadOnlyList<AvanceResumenDto>>(Avances);
    }

    public Task<IReadOnlyList<FotografiaDto>> ObtenerFotografiasAsync(int idCultivo, int idUsuario,
        CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<FotografiaDto>>(Fotografias);
}

/// <summary>Reloj con una hora fija, para que las pruebas de fechas no dependan del día en que se ejecutan.</summary>
internal sealed class RelojFijo(DateTimeOffset ahoraUtc) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => ahoraUtc;
}
