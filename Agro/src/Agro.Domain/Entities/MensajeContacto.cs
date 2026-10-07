using System;
using System.Collections.Generic;

namespace Agro.Domain.Entities;

public partial class MensajeContacto
{
    public int IdMensaje { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Asunto { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool Atendido { get; set; }

    public DateTime FechaEnvio { get; set; }
}
