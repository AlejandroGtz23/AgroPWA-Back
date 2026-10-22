using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Exceptions;
/// <summary>
/// Credenciales o sesion no validas
/// </summary>
public sealed class NoAutorizadoException(string mensaje) : Exception(mensaje);
