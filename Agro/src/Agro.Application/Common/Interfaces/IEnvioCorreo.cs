using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

/// <summary>Correos que manda la aplicación (Gmail por SMTP; en local, la consola).</summary>
public interface IEnvioCorreo
{
    /// <summary>Manda el enlace para restablecer la contraseña; el token va dentro del enlace.</summary>
    Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default);

    /// <summary>Bienvenida al crear la cuenta, con el enlace para verificar el correo.</summary>
    Task EnviarBienvenidaAsync(string correo, string nombre, string token, CancellationToken ct = default);
}