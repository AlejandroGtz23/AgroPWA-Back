using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class IntentoAcceso
{
    public int IdIntento { get; set; }

    public string Correo { get; set; } = null!;

    public int? IdUsuario { get; set; }

    public bool Exitoso { get; set; }

    public string? Ip { get; set; }

    public string? AgenteUsuario { get; set; }

    public DateTime FechaIntento { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
