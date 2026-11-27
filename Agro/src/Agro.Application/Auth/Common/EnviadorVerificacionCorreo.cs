using Agro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Common;

public sealed class EnviadorVerificacionCorreo(
    IVerificacionCorreoProcedimientos procedimientos,
    IGeneradorJwt generadorJwt,
    IEnvioCorreo envioCorreo)
{
    public async Task EnviarAsync(int idUsuario, CancellationToken ct = default)
    {
        // Token aleatorio de un solo uso, igual que en la recuperación de contraseña
        var (token, hash) = generadorJwt.GenerarRefreshToken();
        var codigo = await procedimientos.CrearCodigoAsync(idUsuario, hash, ct);

        // No se manda si ya está verificado o si pidió más de 3 en una hora
        if (codigo.Enviar)
            await envioCorreo.EnviarBienvenidaAsync(codigo.Correo!, codigo.Nombre!, token, ct);
    }
}
