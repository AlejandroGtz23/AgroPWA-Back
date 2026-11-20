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
    public async Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default)
    {
        var enlace = PlantillasCorreo.EnlaceRecuperacion(configuracion.UrlRestablecer, token);

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress("Agro", configuracion.Remitente));
        mensaje.To.Add(new MailboxAddress(nombre, correo));
        mensaje.Subject = PlantillasCorreo.AsuntoRecuperacion;
        mensaje.Body = new BodyBuilder { HtmlBody = PlantillasCorreo.Recuperacion(nombre, enlace) }.ToMessageBody();

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
            // Responder distinto revelaría que el correo está registrado: solo se anota para revisarlo
            logger.LogError(ex, "No se pudo enviar el correo de recuperación");
        }
    }
}
