using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Common;

public sealed record PerfilDto(
string Nombre, string ApellidoPaterno, string? ApellidoMaterno, string Correo,
string? Telefono, string? ComunidadMunicipio, string Rol, DateTimeOffset FechaRegistro);
