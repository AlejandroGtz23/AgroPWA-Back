using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.RegistrarDispositivo;

public sealed record RegistrarDispositivoCommand(string Identificador, string Nombre);
