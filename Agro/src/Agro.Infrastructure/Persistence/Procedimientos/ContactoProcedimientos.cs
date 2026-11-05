using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Persistence.Procedimientos;

public sealed class ContactoProcedimientos(AgroDbContext db) : ProcedimientosBase(db), IContactoProcedimientos
{
    public async Task<bool> RegistrarAsync(NuevoMensajeContacto mensaje, int maximoPorHora, CancellationToken ct = default) =>
        // El procedimiento devuelve (p_id_mensaje, p_limite_excedido)
        await EjecutarFilaAsync(
            "CALL sp_registrar_mensaje_contacto(@nombre, @correo, @asunto, @mensaje, @maximo, NULL, NULL)",
            lector => !lector.GetBoolean(lector.GetOrdinal("p_limite_excedido")),
            ct,
            Parametro("nombre", NpgsqlDbType.Varchar, mensaje.Nombre),
            Parametro("correo", NpgsqlDbType.Varchar, mensaje.Correo),
            Parametro("asunto", NpgsqlDbType.Varchar, mensaje.Asunto),
            Parametro("mensaje", NpgsqlDbType.Varchar, mensaje.Mensaje),
            Parametro("maximo", NpgsqlDbType.Integer, maximoPorHora));
}
