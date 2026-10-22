using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record NuevaSesion(
    int IdUsuario, string Jti, string RefreshTokenHash, DateTime? FechaExpiracion, 
    string? Ip, string? AgenteUsuario);
