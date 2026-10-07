using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string? ApellidoMaterno { get; set; }

    public string Correo { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public DateOnly FechaNacimiento { get; set; }

    public string ContrasenaHash { get; set; } = null!;

    public string? ComunidadMunicipio { get; set; }

    public bool CorreoVerificado { get; set; }

    public bool DosPasosActivo { get; set; }

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadoHasta { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public virtual ICollection<CodigoVerificacion> CodigoVerificacion { get; set; } = new List<CodigoVerificacion>();

    public virtual ICollection<ColaSincronizacion> ColaSincronizacion { get; set; } = new List<ColaSincronizacion>();

    public virtual ICollection<Cultivo> Cultivo { get; set; } = new List<Cultivo>();

    public virtual ICollection<Dispositivo> Dispositivo { get; set; } = new List<Dispositivo>();

    public virtual ICollection<Fotografia> Fotografia { get; set; } = new List<Fotografia>();

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Parcela> Parcela { get; set; } = new List<Parcela>();

    public virtual ICollection<Sesion> Sesion { get; set; } = new List<Sesion>();
}
