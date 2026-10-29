using Agro.Application.Cultivos.Common;

namespace Agro.Application.Common.Models;
// DatosCultivo.cs: fila de sp_obtener_detalle_cultivo
public sealed record DatosCultivo(CultivoDto Cultivo, int TotalAvances, int TotalFotografias);