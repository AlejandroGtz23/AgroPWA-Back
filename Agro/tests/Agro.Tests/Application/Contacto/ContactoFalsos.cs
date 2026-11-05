using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;

namespace Agro.Tests.Application.Contacto;

/// <summary>Tabla mensaje_contacto en memoria: cuenta los mensajes por correo como el procedimiento.</summary>
internal sealed class ContactoProcedimientosFalsos : IContactoProcedimientos
{
    public List<NuevoMensajeContacto> Mensajes { get; } = [];

    public Task<bool> RegistrarAsync(NuevoMensajeContacto mensaje, int maximoPorHora, CancellationToken ct = default)
    {
        if (Mensajes.Count(m => m.Correo == mensaje.Correo) >= maximoPorHora)
            return Task.FromResult(false);   // límite excedido

        Mensajes.Add(mensaje);
        return Task.FromResult(true);
    }
}