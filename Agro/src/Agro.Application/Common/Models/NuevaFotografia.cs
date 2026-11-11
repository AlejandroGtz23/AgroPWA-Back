using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record NuevaFotografia(int? IdAvance, Guid? IdLocal, string Ruta, string NombreArchivo, int TamanoKb,
    bool EsPortada, decimal? Latitud, decimal? Longitud);