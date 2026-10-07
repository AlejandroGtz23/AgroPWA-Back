using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Dispositivo
{
    public int IdDispositivo { get; set; }

    public int IdUsuario { get; set; }

    public string Identificador { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool SincronizarAuto { get; set; }

    public bool SoloWifi { get; set; }

    public decimal EspacioUsadoMb { get; set; }

    public DateTime? UltimaSincronizacion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<ColaSincronizacion> ColaSincronizacion { get; set; } = new List<ColaSincronizacion>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Sesion> Sesion { get; set; } = new List<Sesion>();
}
