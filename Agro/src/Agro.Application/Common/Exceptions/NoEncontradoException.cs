using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Exceptions;

public sealed class NoEncontradoException(string mensaje) : Exception(mensaje);