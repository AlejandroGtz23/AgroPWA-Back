using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record DatosPerfil(string Nombre, string ApellidoPaterno, string? ApellidoMaterno,
    string? Telefono, string? ComunidadMunicipio);
