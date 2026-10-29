using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Common;

public sealed record TipoCultivoDto(int Id, string Nombre);

public sealed record CultivoResumenDto(
    int Id, Guid? IdLocal, string Nombre, string Tipo, string Parcela,
    DateOnly FechaSiembra, int DiasCultivo, string Estado, string EstadoSincronizacion, string? PortadaUrl);
public sealed record CultivoDto(
    int Id, Guid? IdLocal, string Nombre, int IdTipoCultivo, string Tipo, string? Variedad,
    int IdParcela, string Parcela, decimal SuperficieHa,
    DateOnly FechaSiembra, DateOnly? FechaCosecha, int DiasCultivo,
    string Estado, string EstadoSincronizacion, string? Observaciones, string? PortadaUrl);

public sealed record AvanceResumenDto(int Id, string Tipo, DateTime FechaActividad, string Descripcion, int Fotos);

public sealed record FotografiaDto(int Id, string Url, DateTime FechaCaptura, bool EsPortada, int? IdAvance);
public sealed record TotalesCultivoDto(int Avances, int Fotografias);
public sealed record DetalleCultivoDto(CultivoDto Cultivo, IReadOnlyList<AvanceResumenDto> Avances,
    IReadOnlyList<FotografiaDto> Fotografias, TotalesCultivoDto Totales);
