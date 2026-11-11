using Agro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Tests.Application.Fotografias;

internal sealed class AlmacenFotosFalso : IAlmacenFotos
{
    public List<string> Subidas { get; } = [];
    public List<string> Eliminadas { get; } = [];

    public Task SubirAsync(string ruta, Stream contenido, string tipoContenido, CancellationToken ct = default)
    {
        Subidas.Add(ruta);
        return Task.CompletedTask;
    }

    public Task EliminarAsync(string ruta, CancellationToken ct = default)
    {
        Eliminadas.Add(ruta);
        return Task.CompletedTask;
    }

    public string ObtenerUrl(string ruta) => $"https://almacen.test/{ruta}?sig=prueba";
}

