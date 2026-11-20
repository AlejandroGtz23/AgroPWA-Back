using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Commands.RestablecerContrasena;

public sealed record RestablecerContrasenaCommand(string Token, string ContrasenaNueva);
