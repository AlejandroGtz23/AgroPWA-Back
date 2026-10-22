using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Commands.RenovarSesion;
public sealed record RenovarSesionCommand(string RefreshToken);
