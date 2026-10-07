using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class ColaSincronizacion
{
    public int IdRegistro { get; set; }

    public int IdUsuario { get; set; }

    public int IdDispositivo { get; set; }

    public string Entidad { get; set; } = null!;

    public Guid IdLocal { get; set; }

    public string Operacion { get; set; } = null!;

    public string DatosJson { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int Intentos { get; set; }

    public string? MensajeError { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaSincronizacion { get; set; }

    public virtual Dispositivo Dispositivo { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
