using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Commands.CambiarContrasena;

public sealed record CambiarContrasenaCommand(string ContrasenaActual, string ContrasenaNueva);