using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IFotografiasProcedimientos
{
    Task<FotografiaRegistrada> RegistrarAsync(int idUsuario, int idCultivo, NuevaFotografia datos, CancellationToken ct = default);

    Task<DatosFotografia?> ObtenerAsync(int idFotografia, int idUsuario, CancellationToken ct = default);
    /// <summary>Borra la foto del usuario y devuelve su ruta en el almacenamiento; null si no existe o no es suya.</summary>
    Task<string?> EliminarAsync(int idFotografia, int idUsuario, CancellationToken ct = default);
}
