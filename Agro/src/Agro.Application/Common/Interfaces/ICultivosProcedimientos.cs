using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface ICultivosProcedimientos
{
    Task<IReadOnlyList<TipoCultivoDto>> ObtenerTiposAsync(CancellationToken ct = default);

    Task<CultivoRegistrado> RegistrarAsync(int idUsuario, Guid? idLocal, NuevoCultivo datos, CancellationToken ct = default);

    Task<Pagina<CultivoResumenDto>> ObtenerAsync(int idUsuario, FiltroCultivos filtro, CancellationToken ct = default);

    Task<DatosCultivo?> ObtenerDetalleAsync(int idCultivo, int idUsuario, CancellationToken ct = default);

    Task<IReadOnlyList<AvanceResumenDto>> ObtenerAvancesAsync(int idCultivo, int idUsuario, CancellationToken ct = default);

    Task<IReadOnlyList<FotografiaDto>> ObtenerFotografiasAsync(int idCultivo, int idUsuario, CancellationToken ct = default);
}
