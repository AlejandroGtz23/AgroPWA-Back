using Agro.Application.Auth.Commands.ReenviarVerificacion;
using Agro.Application.Auth.Commands.RestablecerContrasena;
using Agro.Application.Auth.Commands.VerificarCorreo;
using Agro.Application.Auth.Common;
using FluentValidation;

namespace Agro.Tests.Application.Auth;

/// <summary>Pruebas de la verificación del correo (HU-32, criterios 2 a 4).</summary>
public class VerificacionCorreoTests
{
    private const int Usuario = 7;
    private const string Token = "refresh-nuevo";   // el que genera GeneradorJwtFalso

    private readonly VerificacionCorreoProcedimientosFalsos _procedimientos = new();
    private readonly EnvioCorreoFalso _correo = new();

    private EnviadorVerificacionCorreo Enviador() => new(_procedimientos, new GeneradorJwtFalso(), _correo);
    private VerificarCorreoHandler Verificar() => new(_procedimientos, new GeneradorJwtFalso(), new VerificarCorreoValidator());
    private ReenviarVerificacionHandler Reenviar() => new(Enviador());

    // ----- Enviar y reenviar -----

    [Fact]
    public async Task Reenviar_manda_un_enlace_nuevo_si_el_correo_no_esta_verificado()
    {
        await Reenviar().EjecutarAsync(Usuario);

        var bienvenida = Assert.Single(_correo.Bienvenidas);
        Assert.Equal("victor@correo.com", bienvenida.Correo);
        Assert.Equal(Token, bienvenida.Token);
    }

    [Fact]
    public async Task Si_el_correo_ya_esta_verificado_no_manda_nada()
    {
        _procedimientos.YaVerificado = true;

        await Reenviar().EjecutarAsync(Usuario);

        Assert.Empty(_correo.Bienvenidas);
    }

    // ----- Verificar -----

    [Fact]
    public async Task Verifica_el_correo_con_un_token_vigente()
    {
        await Enviador().EnviarAsync(Usuario);

        await Verificar().EjecutarAsync(new VerificarCorreoCommand(Token));

        Assert.True(_procedimientos.Verificado);
    }

    [Fact]
    public async Task El_mismo_token_no_sirve_dos_veces()
    {
        await Enviador().EnviarAsync(Usuario);
        await Verificar().EjecutarAsync(new VerificarCorreoCommand(Token));

        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Verificar().EjecutarAsync(new VerificarCorreoCommand(Token)));

        var campo = Assert.Single(error.Errors);
        Assert.Equal("token", campo.PropertyName);
        Assert.Equal(RestablecerContrasenaHandler.EnlaceInvalido, campo.ErrorMessage);
    }

    [Fact]
    public async Task Un_token_que_no_existe_responde_400()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Verificar().EjecutarAsync(new VerificarCorreoCommand("inventado")));

        Assert.False(_procedimientos.Verificado);
    }

    [Fact]
    public void Sin_token_el_enlace_no_es_valido()
    {
        var error = Assert.Single(new VerificarCorreoValidator().Validate(new VerificarCorreoCommand("")).Errors);

        Assert.Equal(RestablecerContrasenaHandler.EnlaceInvalido, error.ErrorMessage);
    }

    [Fact]
    public async Task Sin_token_no_llega_a_la_base_de_datos()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Verificar().EjecutarAsync(new VerificarCorreoCommand("  ")));

        Assert.False(_procedimientos.Verificado);
    }
}