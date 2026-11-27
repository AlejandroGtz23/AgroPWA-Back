using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IVerificacionCorreoProcedimientos
{
    /// <summary>Guarda el hash del token si el correo no está verificado (máx. 3 por hora).</summary>
    Task<CodigoVerificacionCorreo> CrearCodigoAsync(int idUsuario, string codigoHash, CancellationToken ct = default);

    /// <summary>false si el token no existe, venció o ya se usó.</summary>
    Task<bool> VerificarAsync(string codigoHash, CancellationToken ct = default);
}
