using Agro.Application.Common.Interfaces;

namespace Agro.Application.Salud.Queries.ComprobarSalud;

/// <summary>
/// Query (solo lectura): comprueba que la API y la base de datos respondan.
/// Criterio 3 de la HU-04: GET /api/salud responde 200 con estado «OK».
/// </summary>
public class ComprobarSaludHandler(IAgroDbContext db)
{
    public async Task<SaludDto> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        bool conectada;
        try
        {
            conectada = await db.PuedeConectarAsync(cancellationToken);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            conectada = false;
        }

        return conectada
            ? new SaludDto("OK", "conectada", DateTime.UtcNow)
            : new SaludDto("ERROR", "sin conexión", DateTime.UtcNow,
                "La API funciona, pero no hay conexión con la base de datos.");
    }
}
