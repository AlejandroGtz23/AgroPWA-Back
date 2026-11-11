using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;

namespace Agro.Tests.Application.Fotografias;

/// <summary>Tabla fotografia en memoria: idLocal sin duplicar, solo las fotos del usuario y borrar devuelve la ruta.</summary>
internal sealed class FotografiasProcedimientosFalsos : IFotografiasProcedimientos
{
    private readonly List<(int IdUsuario, DatosFotografia Foto)> _fotos = [];

    /// <summary>Si se asigna, RegistrarAsync lo lanza (como el procedimiento ante un cultivo ajeno o la 6.ª foto).</summary>
    public Exception? ErrorAlRegistrar { get; set; }

    public IReadOnlyList<DatosFotografia> Fotos => [.. _fotos.Select(f => f.Foto)];

    public Task<FotografiaRegistrada> RegistrarAsync(int idUsuario, int idCultivo, NuevaFotografia datos,
        CancellationToken ct = default)
    {
        if (ErrorAlRegistrar is not null)
            throw ErrorAlRegistrar;

        var existente = _fotos.FirstOrDefault(f =>
            f.IdUsuario == idUsuario && datos.IdLocal is not null && f.Foto.IdLocal == datos.IdLocal);
        if (existente.Foto is not null)
            return Task.FromResult(new FotografiaRegistrada(existente.Foto.Id, YaExistia: true));

        var foto = new DatosFotografia(_fotos.Count + 1, datos.IdLocal, idCultivo, datos.IdAvance, datos.Ruta,
            datos.NombreArchivo, datos.TamanoKb, datos.EsPortada, datos.Latitud, datos.Longitud, DateTimeOffset.UnixEpoch);
        _fotos.Add((idUsuario, foto));
        return Task.FromResult(new FotografiaRegistrada(foto.Id, YaExistia: false));
    }

    public Task<DatosFotografia?> ObtenerAsync(int idFotografia, int idUsuario, CancellationToken ct = default) =>
        Task.FromResult<DatosFotografia?>(
            _fotos.FirstOrDefault(f => f.IdUsuario == idUsuario && f.Foto.Id == idFotografia).Foto);

    public Task<string?> EliminarAsync(int idFotografia, int idUsuario, CancellationToken ct = default)
    {
        var i = _fotos.FindIndex(f => f.IdUsuario == idUsuario && f.Foto.Id == idFotografia);
        if (i < 0)
            return Task.FromResult<string?>(null);

        var ruta = _fotos[i].Foto.Ruta;
        _fotos.RemoveAt(i);
        return Task.FromResult<string?>(ruta);
    }
}