using Agro.Application.Auth.Commands.CerrarSesion;
using Agro.Application.Auth.Commands.RenovarSesion;
using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using FluentValidation;

namespace Agro.Tests.Application.Auth;

/// <summary>Pruebas de la sesión: creación, validación, renovación (HU-10) y cierre (HU-11).</summary>
public class SesionTests
{
    private readonly ProcedimientosFalsos _procedimientos = new();
    private static readonly UsuarioSesionDto Usuario = new(1, "Victor", "victor@correo.com", "productor");

    // ----- CreadorSesion -----

    [Theory]
    [InlineData(true, 7 * 24)]   // Recordarme: 7 días
    [InlineData(false, 12)]      // Normal: 12 horas
    public async Task La_sesion_vence_segun_Recordarme(bool recordarme, int horas)
    {
        var creador = new CreadorSesion(new GeneradorJwtFalso(), _procedimientos, TimeProvider.System);
        var esperado = DateTime.UtcNow.AddHours(horas);

        var sesion = await creador.CrearAsync(Usuario, recordarme, "1.1.1.1", "xunit");

        var guardada = _procedimientos.SesionCreada!;
        Assert.InRange(guardada.FechaExpiracion!.Value, esperado.AddMinutes(-1), esperado.AddMinutes(1));
        Assert.Equal("hash-refresh-nuevo", guardada.RefreshTokenHash);   // en la base solo va el hash
        Assert.Equal("refresh-nuevo", sesion.RefreshToken);              // al cliente va el token
        Assert.Equal(32, guardada.Jti.Length);
        Assert.Same(Usuario, sesion.Usuario);
    }

    // ----- ValidadorSesion -----

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Validar_sesion_responde_lo_que_dice_el_procedimiento(bool valida)
    {
        _procedimientos.SesionValida = valida;

        var resultado = await new ValidadorSesion(_procedimientos).EsValidaAsync("jti");

        Assert.Equal(valida, resultado);
        Assert.Equal(ReglasSesion.MinutosInactividad, _procedimientos.MinutosRecibidos);
    }

    // ----- RenovarSesion -----

    private RenovarSesionHandler CrearRenovar()
        => new(_procedimientos, new GeneradorJwtFalso(), new RenovarSesionValidator());

    [Fact]
    public async Task Renovar_entrega_tokens_nuevos_buscando_por_el_hash()
    {
        _procedimientos.UsuarioRenovado = Usuario;

        var sesion = await CrearRenovar().EjecutarAsync(new RenovarSesionCommand("refresh-viejo"));

        Assert.Equal("hash-refresh-viejo", _procedimientos.HashRenovado);
        Assert.Equal("token-acceso", sesion.AccessToken);
        Assert.Equal("refresh-nuevo", sesion.RefreshToken);
        Assert.Equal(Usuario, sesion.Usuario);
    }

    [Fact]
    public async Task Renovar_con_sesion_expirada_lanza_401()
    {
        _procedimientos.UsuarioRenovado = null;

        await Assert.ThrowsAsync<NoAutorizadoException>(
            () => CrearRenovar().EjecutarAsync(new RenovarSesionCommand("refresh-viejo")));
    }

    [Fact]
    public async Task Renovar_sin_refresh_token_lanza_error_de_validacion()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => CrearRenovar().EjecutarAsync(new RenovarSesionCommand("")));
    }

    // ----- CerrarSesion -----

    [Fact]
    public async Task Cerrar_sesion_abierta_no_lanza_error()
    {
        _procedimientos.SesionCerrada = true;

        var error = await Record.ExceptionAsync(
            () => new CerrarSesionHandler(_procedimientos).EjecutarAsync(new CerrarSesionCommand("jti")));

        Assert.Null(error);
    }

    [Fact]
    public async Task Cerrar_sesion_ya_cerrada_lanza_401()
    {
        _procedimientos.SesionCerrada = false;

        await Assert.ThrowsAsync<NoAutorizadoException>(
            () => new CerrarSesionHandler(_procedimientos).EjecutarAsync(new CerrarSesionCommand("jti")));
    }
}
