using Agro.Application.Common.Models;

namespace Agro.Application.Common.Interfaces;

public interface IRecuperacionProcedimientos
{
    /// <summary>Guarda el hash del token si el correo es de un usuario activo (máx. 3 por hora).</summary>
    Task<CodigoRecuperacion> CrearCodigoAsync(string correo, string codigoHash, CancellationToken ct = default);

    /// <summary>false si el token no existe, venció o ya se usó.</summary>
    Task<bool> RestablecerAsync(string codigoHash, string nuevoHash, CancellationToken ct = default);
}
