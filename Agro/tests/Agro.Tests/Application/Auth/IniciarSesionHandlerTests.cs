using Agro.Application.Auth.Commands.IniciarSesion;
using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Models;
using FluentValidation;

namespace Agro.Tests.Application.Auth;

/// <summary>Pruebas del inicio de sesión (HU-10).</summary>
public class IniciarSesionHandlerTests
{
    private readonly ProcedimientosFalsos _procedimientos = new();

    private IniciarSesionHandler CrearHandler() => new(
        _procedimientos, new HashFalso(),
        new CreadorSesion(new GeneradorJwtFalso(), _procedimientos, TimeProvider.System),
        new IniciarSesionValidator(), TimeProvider.System);

    private static UsuarioLogin Usuario(DateTime? bloqueadoHasta = null)
        => new(1, "Victor", "victor@correo.com", "hash:Secreta123", "productor", bloqueadoHasta);

    [Fact]
    public async Task Con_credenciales_correctas_devuelve_la_sesion()
    {
        _procedimientos.UsuarioLogin = Usuario();

        var sesion = await CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("Victor@Correo.com", "Secreta123", false), "1.1.1.1", "xunit");

        Assert.Equal("token-acceso", sesion.AccessToken);
        Assert.Equal("refresh-nuevo", sesion.RefreshToken);
        Assert.Equal(1, sesion.Usuario.Id);
        Assert.Equal([true], _procedimientos.Intentos);
        Assert.NotNull(_procedimientos.SesionCreada);
    }

    [Fact]
    public async Task Con_contrasena_incorrecta_lanza_401_y_registra_el_intento_fallido()
    {
        _procedimientos.UsuarioLogin = Usuario();

        await Assert.ThrowsAsync<NoAutorizadoException>(() => CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("victor@correo.com", "Incorrecta", false), null, null));

        Assert.Equal([false], _procedimientos.Intentos);
        Assert.Null(_procedimientos.SesionCreada);
    }

    [Fact]
    public async Task Si_el_correo_no_existe_lanza_401_y_registra_el_intento()
    {
        await Assert.ThrowsAsync<NoAutorizadoException>(() => CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("nadie@correo.com", "Secreta123", false), null, null));

        Assert.Equal([false], _procedimientos.Intentos);
    }

    [Fact]
    public async Task Si_la_cuenta_esta_bloqueada_lanza_429_sin_registrar_intento()
    {
        _procedimientos.UsuarioLogin = Usuario(bloqueadoHasta: DateTime.UtcNow.AddMinutes(10));

        await Assert.ThrowsAsync<DemasiadosIntentosException>(() => CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("victor@correo.com", "Secreta123", false), null, null));

        Assert.Empty(_procedimientos.Intentos);
    }

    [Fact]
    public async Task Si_el_bloqueo_ya_paso_permite_entrar()
    {
        _procedimientos.UsuarioLogin = Usuario(bloqueadoHasta: DateTime.UtcNow.AddMinutes(-1));

        var sesion = await CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("victor@correo.com", "Secreta123", false), null, null);

        Assert.Equal("token-acceso", sesion.AccessToken);
    }

    [Fact]
    public async Task Con_datos_vacios_lanza_error_de_validacion()
    {
        await Assert.ThrowsAsync<ValidationException>(() => CrearHandler().EjecutarAsync(
            new IniciarSesionCommand("", "", false), null, null));

        Assert.Empty(_procedimientos.Intentos);
    }
}
