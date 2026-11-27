using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Configuration;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;


namespace Agro.Infrastructure.Correo;

/// <summary>Manda los correos por SMTP con STARTTLS (Gmail: smtp.gmail.com, puerto 587, contraseña de aplicación).</summary>
public sealed class EnvioCorreoSmtp(ConfiguracionCorreo configuracion, ILogger<EnvioCorreoSmtp> logger) : IEnvioCorreo
{
    public Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default) =>
        EnviarAsync(correo, nombre, PlantillasCorreo.AsuntoRecuperacion,
            PlantillasCorreo.Recuperacion(nombre, PlantillasCorreo.EnlaceConToken(configuracion.UrlRestablecer, token)), ct);

    public Task EnviarBienvenidaAsync(string correo, string nombre, string token, CancellationToken ct = default) =>
        EnviarAsync(correo, nombre, PlantillasCorreo.AsuntoBienvenida,
            PlantillasCorreo.Bienvenida(nombre, PlantillasCorreo.EnlaceConToken(configuracion.UrlVerificarCorreo, token)), ct);

    private async Task EnviarAsync(string correo, string nombre, string asunto, string html, CancellationToken ct)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress("Agro", configuracion.Remitente));
        mensaje.To.Add(new MailboxAddress(nombre, correo));
        mensaje.Subject = asunto;
        mensaje.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        try
        {
            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(configuracion.Host!, configuracion.Puerto, SecureSocketOptions.StartTls, ct);
            await cliente.AuthenticateAsync(configuracion.Usuario!, configuracion.Contrasena!, ct);
            await cliente.SendAsync(mensaje, ct);
            await cliente.DisconnectAsync(quit: true, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Un correo que no sale no debe romper el registro ni revelar si una cuenta existe: solo se anota
            logger.LogError(ex, "No se pudo enviar el correo «{Asunto}»", asunto);
        }
    }
}