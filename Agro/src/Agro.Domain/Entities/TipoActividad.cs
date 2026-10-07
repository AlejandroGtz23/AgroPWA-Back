using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class TipoActividad
{
    public int IdTipoActividad { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Avance> Avance { get; set; } = new List<Avance>();
}
