
using Agro.Application.Common.Interfaces;


namespace Agro.Application.Auth.Common;
/// <summary>Comprueba en cada petición que la sesión del token siga abierta (sp_validar_sesion).</summary>

public sealed class ValidadorSesion(IAuthProcedimientos procedimientos)
{
    public Task<bool> EsValidaAsync(string jti, CancellationToken ct = default) =>
        procedimientos.ValidarSesionAsync(jti, ReglasSesion.MinutosInactividad, ct);
}
