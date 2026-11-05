using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Contacto.Commands.EnviarMensajeContacto;

public sealed record EnviarMensajeContactoCommand(string Nombre, string Correo, string? Asunto, string Mensaje);