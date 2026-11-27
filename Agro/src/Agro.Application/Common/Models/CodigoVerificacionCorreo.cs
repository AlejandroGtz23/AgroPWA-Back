using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record CodigoVerificacionCorreo(bool Enviar, string? Nombre, string? Correo);