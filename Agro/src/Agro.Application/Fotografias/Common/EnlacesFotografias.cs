using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;

namespace Agro.Application.Fotografias.Common;

/// <summary>
/// La BD guarda la ruta de cada foto (archivo_url); el frontend necesita un enlace que pueda abrir.
/// Convierte las rutas de las respuestas en enlaces temporales del almacén.
/// </summary>
public sealed class EnlacesFotografias(IAlmacenFotos almacen)
{
    public string? Enlace(string? ruta) => ruta is null ? null : almacen.ObtenerUrl(ruta);

    public CultivoDto ConEnlaces(CultivoDto cultivo) =>
        cultivo with { PortadaUrl = Enlace(cultivo.PortadaUrl) };

    public FotografiaDto ConEnlace(FotografiaDto foto) =>
        foto with { Url = almacen.ObtenerUrl(foto.Url) };

    public Pagina<CultivoResumenDto> ConEnlaces(Pagina<CultivoResumenDto> pagina) =>
        pagina with { Elementos = [.. pagina.Elementos.Select(c => c with { PortadaUrl = Enlace(c.PortadaUrl) })] };
}