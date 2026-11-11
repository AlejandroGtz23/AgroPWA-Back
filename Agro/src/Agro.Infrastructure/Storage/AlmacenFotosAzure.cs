using Agro.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Storage;

public sealed class AlmacenFotosAzure(BlobContainerClient contenedor, TimeProvider reloj) : IAlmacenFotos
{
    private static readonly TimeSpan VigenciaEnlace = TimeSpan.FromHours(1);

    public async Task SubirAsync(string ruta, Stream contenido, string tipoContenido, CancellationToken ct = default)
    {
        // En Azure el contenedor ya existe; en Azurite se crea la primera vez
        await contenedor.CreateIfNotExistsAsync(cancellationToken: ct);
        await contenedor.GetBlobClient(ruta).UploadAsync(contenido,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = tipoContenido } }, ct);
    }

    public async Task EliminarAsync(string ruta, CancellationToken ct = default) =>
        await contenedor.GetBlobClient(ruta).DeleteIfExistsAsync(cancellationToken: ct);

    public string ObtenerUrl(string ruta) =>
        contenedor.GetBlobClient(ruta)
            .GenerateSasUri(BlobSasPermissions.Read, reloj.GetUtcNow().Add(VigenciaEnlace))
            .ToString();
}