using System;
using System.Collections.Generic;
using System.Text;

using System.Net;

namespace Agro.Infrastructure.Correo;

/// <summary>Enlace y texto de los correos (los usan el envío por SMTP y el de consola).</summary>
public static class PlantillasCorreo
{
    public const string AsuntoRecuperacion = "Restablece tu contraseña de Agro";

    public static string EnlaceRecuperacion(string urlRestablecer, string token) =>
        $"{urlRestablecer}?token={Uri.EscapeDataString(token)}";

    public static string Recuperacion(string nombre, string enlace) => $"""
        <p>Hola, {WebUtility.HtmlEncode(nombre)}:</p>
        <p>Recibimos una solicitud para restablecer tu contraseña de <strong>Agro</strong>.</p>
        <p><a href="{enlace}">Restablecer mi contraseña</a></p>
        <p>El enlace vence en <strong>30 minutos</strong> y solo funciona una vez.</p>
        <p>Si no lo pediste tú, ignora este correo: tu contraseña no cambia.</p>
        """;
}
