using Agro.Application.Avances.Common;
using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IAvancesProcedimientos
{
    Task<IReadOnlyList<TipoActividadDto>> ObtenerTiposAsync(CancellationToken ct = default);
    Task<AvanceRegistrado> RegistrarAsync(int idUsuario, int idCultivo, Guid? idLocal, NuevoAvance datos, CancellationToken ct = default);

    Task<AvanceDto?> ObtenerAsync(int idAvance, int idUsuario, CancellationToken ct = default);
}
