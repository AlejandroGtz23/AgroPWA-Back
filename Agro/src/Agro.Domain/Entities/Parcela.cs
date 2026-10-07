using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Parcela
{
    public int IdParcela { get; set; }

    public int IdUsuario { get; set; }

    public Guid? IdLocal { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal SuperficieHa { get; set; }

    public string? UbicacionReferencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual ICollection<Cultivo> Cultivo { get; set; } = new List<Cultivo>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
