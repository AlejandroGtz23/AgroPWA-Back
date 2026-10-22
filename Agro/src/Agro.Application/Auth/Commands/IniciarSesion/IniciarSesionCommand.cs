using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Commands.IniciarSesion;

public sealed record IniciarSesionCommand(string Correo, string Contrasena, bool Recordarme);

