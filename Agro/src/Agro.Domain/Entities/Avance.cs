using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Avance
{
    public int IdAvance { get; set; }

    public int IdCultivo { get; set; }

    public int IdTipoActividad { get; set; }

    public int IdUsuario { get; set; }

    public Guid? IdLocal { get; set; }

    public DateTime FechaActividad { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? DuracionMin { get; set; }

    public string? MetodoRiego { get; set; }

    public string? Producto { get; set; }

    public decimal? Cantidad { get; set; }

    public string? Unidad { get; set; }

    public string EstadoSincronizacion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual Cultivo Cultivo { get; set; } = null!;

    public virtual ICollection<Fotografia> Fotografia { get; set; } = new List<Fotografia>();

    public virtual TipoActividad IdTipoActividadNavigation { get; set; } = null!;
}
