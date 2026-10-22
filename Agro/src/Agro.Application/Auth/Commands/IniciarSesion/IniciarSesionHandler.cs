using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using FluentValidation;

namespace Agro.Application.Auth.Commands.IniciarSesion;
/// <summary>
/// Inicia sesion con correo y contraseña
/// </summary>
public sealed class IniciarSesionHandler(
    IAuthProcedimientos procedimientos,
    IHashContrasena hashContrasena,
    CreadorSesion creadorSesion,
    IValidator<IniciarSesionCommand> validador,
    TimeProvider reloj)
{
    private const string CredencialesIncorrectas = "Correo o contraseña incorrectos.";

    public async Task<SesionDto> EjecutarAsync(IniciarSesionCommand comando,
        string? ip, string? agenteUsuario, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                          // 400

        var correo = comando.Correo.Trim().ToLowerInvariant();
        var usuario = await procedimientos.ObtenerUsuarioLoginAsync(correo, ct);   // sp_obtener_usuario_login

        if (usuario?.BloqueadoHasta > reloj.GetUtcNow().UtcDateTime)               // 429
            throw new DemasiadosIntentosException("Demasiados intentos. Intenta en 15 minutos.");

        // La contraseña se compara en C# con BCrypt: nunca viaja a la base en texto plano
        var correcta = usuario is not null && hashContrasena.Verificar(comando.Contrasena, usuario.ContrasenaHash);

        await procedimientos.RegistrarIntentoAccesoAsync(                          // sp_registrar_intento_acceso
            correo, usuario?.IdUsuario, correcta, ip, agenteUsuario, ct);

        // Mismo mensaje si el correo no existe o la contraseña está mal (criterio 2)
        if (!correcta)
            throw new NoAutorizadoException(CredencialesIncorrectas);              // 401

        var datos = new UsuarioSesionDto(usuario!.IdUsuario, usuario.Nombre, usuario.Correo, usuario.Rol);
        return await creadorSesion.CrearAsync(datos, comando.Recordarme, ip, agenteUsuario, ct); // sp_crear_sesion
    }
}
