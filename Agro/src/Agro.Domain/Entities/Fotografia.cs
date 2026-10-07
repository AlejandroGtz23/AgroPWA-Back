using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class Fotografia
{
    public int IdFotografia { get; set; }

    public int IdUsuario { get; set; }

    public int IdCultivo { get; set; }

    public int? IdAvance { get; set; }

    public Guid? IdLocal { get; set; }

    public string ArchivoUrl { get; set; } = null!;

    public string NombreArchivo { get; set; } = null!;

    public int TamanoKb { get; set; }

    public bool EsPortada { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public DateTime FechaCaptura { get; set; }

    public string EstadoSincronizacion { get; set; } = null!;

    public virtual Avance? Avance { get; set; }

    public virtual Cultivo Cultivo { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
