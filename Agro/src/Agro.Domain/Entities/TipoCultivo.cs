using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class TipoCultivo
{
    public int IdTipoCultivo { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Cultivo> Cultivo { get; set; } = new List<Cultivo>();
}
