using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Sesion
{
    public int IdSesion { get; set; }

    public int IdUsuario { get; set; }

    public int? IdDispositivo { get; set; }

    public string Jti { get; set; } = null!;

    public string RefreshTokenHash { get; set; } = null!;

    public string? Ip { get; set; }

    public string? AgenteUsuario { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime UltimaActividad { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public string? MotivoCierre { get; set; }

    public virtual Dispositivo? IdDispositivoNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
