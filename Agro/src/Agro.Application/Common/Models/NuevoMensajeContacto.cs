using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record NuevoMensajeContacto(string Nombre, string Correo, string? Asunto, string Mensaje);