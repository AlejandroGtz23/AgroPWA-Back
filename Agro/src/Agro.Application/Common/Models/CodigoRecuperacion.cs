using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

/// <summary>Resultado de sp_crear_codigo_recuperacion: si hay que mandar el correo y a qué nombre.</summary>
public sealed record CodigoRecuperacion(bool Enviar, string? Nombre);