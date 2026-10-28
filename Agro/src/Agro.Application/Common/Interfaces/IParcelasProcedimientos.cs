using Agro.Application.Common.Models;
using Agro.Application.Parcelas.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IParcelasProcedimientos
{
    Task<ParcelaRegistrada> RegistrarAsync(int idUsuario, Guid? idLocal, DatosParcela datos, CancellationToken ct = default);
    Task<bool> EditarAsync(int idParcela, int idUsuario, DatosParcela datos, CancellationToken ct = default);

    Task<IReadOnlyList<ParcelaDto>> ObtenerAsync(int idUsuario, int? idParcela = null, CancellationToken ct = default);
}
