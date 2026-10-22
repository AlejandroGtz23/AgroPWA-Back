using Microsoft.OpenApi;

namespace Agro.Api.Configuration;

/// <summary>Documento OpenAPI con título y el botón «Authorize» de Swagger.</summary>
public static class DocumentacionApi
{
    public static IServiceCollection AddDocumentacionApi(this IServiceCollection services) =>
        services.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, contexto, ct) =>
        {
            documento.Info.Title = "Agro API";
            documento.Info.Version = "v1";
            documento.Info.Description = "API de Agro, la PWA para el registro y seguimiento de cultivos.";

            // Swagger muestra el botón Authorize para pegar el accessToken
            documento.Components ??= new OpenApiComponents();
            documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pega aquí el accessToken (sin la palabra Bearer)."
            };
            documento.Security ??= [];
            documento.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
            });
            return Task.CompletedTask;
        }));
}