using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Fotografias.Commands.SubirFotografia;
/// <summary>
/// Foto que llega del formulario. No usa IFormFile (eso es de ASP.NET): el controlador
/// pasa el contenido como Stream y su tamaño en bytes.
/// </summary>

public sealed record SubirFotografiaCommand(Stream Contenido, long TamanoBytes, int? IdAvance, bool EsPortada,
    Guid? IdLocal, decimal? Latitud, decimal? Longitud);

