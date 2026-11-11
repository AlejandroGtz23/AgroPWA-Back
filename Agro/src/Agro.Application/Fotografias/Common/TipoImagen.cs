using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Fotografias.Common;

public static class TipoImagen
{
    private static readonly byte[] FirmaJpg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] FirmaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Devuelve (tipo MIME, extensión) o null si no es JPG ni PNG. Deja el stream al inicio.</summary>
    public static async Task<(string Tipo, string Extension)?> DetectarAsync(Stream contenido, CancellationToken ct = default)
    {
        var inicio = new byte[FirmaPng.Length];
        var leidos = await contenido.ReadAtLeastAsync(inicio, inicio.Length, throwOnEndOfStream: false, ct);
        contenido.Position = 0;

        if (leidos >= FirmaPng.Length && inicio.AsSpan().SequenceEqual(FirmaPng)) return ("image/png", ".png");
        if (leidos >= FirmaJpg.Length && inicio.AsSpan(0, FirmaJpg.Length).SequenceEqual(FirmaJpg)) return ("image/jpeg", ".jpg");
        return null;
    }
}
