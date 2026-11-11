using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record FotografiaSubidaDto(int Id, Guid? IdLocal, int IdCultivo, int? IdAvance, string Url,
    string NombreArchivo, int TamanoKb, bool EsPortada, decimal? Latitud, decimal? Longitud, DateTimeOffset FechaCaptura);
