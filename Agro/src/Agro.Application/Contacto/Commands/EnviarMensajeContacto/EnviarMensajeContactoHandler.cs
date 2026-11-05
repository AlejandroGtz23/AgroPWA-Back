using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Contacto.Commands.EnviarMensajeContacto;
public sealed class EnviarMensajeContactoHandler(IContactoProcedimientos procedimientos, IValidator<EnviarMensajeContactoCommand> validador)
{
    public const int MaximoPorHora = 3;
    public const string LimiteExcedido = "Ya enviaste varios mensajes. Intenta más tarde.";
    public async Task EjecutarAsync(EnviarMensajeContactoCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                  // 400

        var mensaje = new NuevoMensajeContacto(
            comando.Nombre.Trim(), comando.Correo.Trim().ToLowerInvariant(),
            ReglasCultivo.Limpiar(comando.Asunto), comando.Mensaje.Trim());

        if (!await procedimientos.RegistrarAsync(mensaje, MaximoPorHora, ct))
            throw new DemasiadosIntentosException(LimiteExcedido);                          // 429
    }

}
