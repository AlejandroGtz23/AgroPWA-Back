using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record DatosFotografia(int Id, Guid? IdLocal, int IdCultivo, int? IdAvance, string Ruta,
    string NombreArchivo, int TamanoKb, bool EsPortada, decimal? Latitud, decimal? Longitud, DateTimeOffset FechaCaptura);