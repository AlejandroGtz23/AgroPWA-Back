using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Commands.ActualizarPerfil;

public sealed record ActualizarPerfilCommand(string Nombre, string ApellidoPaterno, string? ApellidoMaterno,
    string? Telefono, string? ComunidadMunicipio);
