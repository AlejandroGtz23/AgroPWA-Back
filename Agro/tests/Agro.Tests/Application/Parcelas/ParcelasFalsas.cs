using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Parcelas.Common;

namespace Agro.Tests.Application.Parcelas;

/// <summary>
/// Simula los procedimientos de parcelas (migración 011) en memoria, con las mismas reglas:
/// idLocal repetido devuelve la existente, nombre único por usuario (sin mayúsculas) y
/// cada usuario solo ve y edita sus parcelas.
/// </summary>
internal sealed class ParcelasProcedimientosFalsos : IParcelasProcedimientos
{
    private readonly List<(int IdUsuario, ParcelaDto Parcela)> _parcelas = [];

    public int LlamadasRegistrar { get; private set; }
    public int? UsuarioConsultado { get; private set; }
    public DatosParcela? UltimosDatos { get; private set; }

    public Task<ParcelaRegistrada> RegistrarAsync(int idUsuario, Guid? idLocal, DatosParcela datos,
        CancellationToken ct = default)
    {
        LlamadasRegistrar++;
        UltimosDatos = datos;

        var existente = _parcelas.FirstOrDefault(p =>
            p.IdUsuario == idUsuario && idLocal is not null && p.Parcela.IdLocal == idLocal);
        if (existente.Parcela is not null)
            return Task.FromResult(new ParcelaRegistrada(existente.Parcela.Id, YaExistia: true));

        ValidarNombreUnico(idUsuario, datos.Nombre, idExcluida: null);

        var parcela = new ParcelaDto(_parcelas.Count + 1, idLocal, datos.Nombre, datos.SuperficieHa,
            datos.UbicacionReferencia, NumeroCultivos: 0);
        _parcelas.Add((idUsuario, parcela));
        return Task.FromResult(new ParcelaRegistrada(parcela.Id, YaExistia: false));
    }

    public Task<bool> EditarAsync(int idParcela, int idUsuario, DatosParcela datos, CancellationToken ct = default)
    {
        UltimosDatos = datos;

        var indice = _parcelas.FindIndex(p => p.IdUsuario == idUsuario && p.Parcela.Id == idParcela);
        if (indice < 0)
            return Task.FromResult(false);

        ValidarNombreUnico(idUsuario, datos.Nombre, idExcluida: idParcela);

        _parcelas[indice] = (idUsuario, _parcelas[indice].Parcela with
        {
            Nombre = datos.Nombre,
            SuperficieHa = datos.SuperficieHa,
            UbicacionReferencia = datos.UbicacionReferencia
        });
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<ParcelaDto>> ObtenerAsync(int idUsuario, int? idParcela = null,
        CancellationToken ct = default)
    {
        UsuarioConsultado = idUsuario;
        IReadOnlyList<ParcelaDto> resultado = _parcelas
            .Where(p => p.IdUsuario == idUsuario && (idParcela is null || p.Parcela.Id == idParcela))
            .Select(p => p.Parcela)
            .OrderBy(p => p.Nombre)
            .ToList();
        return Task.FromResult(resultado);
    }

    private void ValidarNombreUnico(int idUsuario, string nombre, int? idExcluida)
    {
        if (_parcelas.Any(p => p.IdUsuario == idUsuario && p.Parcela.Id != idExcluida
                               && string.Equals(p.Parcela.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictoException("Ya tienes una parcela con ese nombre.");
    }
}
