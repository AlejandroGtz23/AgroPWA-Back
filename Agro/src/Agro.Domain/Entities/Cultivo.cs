using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Cultivo
{
    public int IdCultivo { get; set; }

    public int IdUsuario { get; set; }

    public int IdParcela { get; set; }

    public int IdTipoCultivo { get; set; }

    public Guid? IdLocal { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Variedad { get; set; }

    public DateOnly FechaSiembra { get; set; }

    public DateOnly? FechaCosecha { get; set; }

    public string Estado { get; set; } = null!;

    public string? Observaciones { get; set; }

    public string EstadoSincronizacion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public virtual ICollection<Avance> Avance { get; set; } = new List<Avance>();

    public virtual ICollection<Fotografia> Fotografia { get; set; } = new List<Fotografia>();

    public virtual TipoCultivo IdTipoCultivoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Parcela Parcela { get; set; } = null!;
}
