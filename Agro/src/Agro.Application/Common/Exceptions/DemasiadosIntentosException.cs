using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Exceptions;
/// <summary>
/// Cuenta bloqueada temporalmente por demaciados intentos fallidos 
/// </summary>
public sealed class DemasiadosIntentosException(string mensaje) : Exception(mensaje)
{
}
