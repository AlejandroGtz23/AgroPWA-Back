using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IAlmacenFotos
{
    /// <summary>Guarda el archivo en la ruta indicada, p. ej. usuario-3/cultivo-12/abc.jpg.</summary>
    Task SubirAsync(string ruta, Stream contenido, string tipoContenido, CancellationToken ct = default);

    /// <summary>Borra el archivo; no falla si ya no existe.</summary>
    Task EliminarAsync(string ruta, CancellationToken ct = default);

    /// <summary>Enlace de solo lectura que vence en 1 hora (SAS).</summary>
    string ObtenerUrl(string ruta);
}
