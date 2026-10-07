using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class CodigoVerificacion
{
    public int IdCodigo { get; set; }

    public int IdUsuario { get; set; }

    public string Tipo { get; set; } = null!;

    public string CodigoHash { get; set; } = null!;

    public int Intentos { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public DateTime? FechaUso { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
