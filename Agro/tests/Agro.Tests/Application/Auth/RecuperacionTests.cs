using Agro.Application.Auth.Commands.RestablecerContrasena;
using Agro.Application.Auth.Commands.SolicitarRecuperacion;
using FluentValidation;

namespace Agro.Tests.Application.Auth;

/// <summary>Pruebas de recuperar la contraseña por correo (HU-27, criterios 1 a 4).</summary>
public class RecuperacionTests
{
    private const string Token = "refresh-nuevo";   // el que genera GeneradorJwtFalso

    private readonly RecuperacionProcedimientosFalsos _procedimientos = new();
    private readonly EnvioCorreoFalso _correo = new();

    private SolicitarRecuperacionHandler Solicitar() =>
        new(_procedimientos, new GeneradorJwtFalso(), _correo, new SolicitarRecuperacionValidator());

    private RestablecerContrasenaHandler Restablecer() =>
        new(_procedimientos, new GeneradorJwtFalso(), new HashFalso(), new RestablecerContrasenaValidator());

    // ----- Solicitar -----

    [Fact]
    public async Task Manda_el_enlace_con_el_token_y_el_correo_en_minusculas()
    {
        var respuesta = await Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand("  ANA@agro.test "));

        Assert.Equal(SolicitarRecuperacionHandler.Respuesta, respuesta.Mensaje);
        Assert.Equal(RecuperacionProcedimientosFalsos.Correo, _procedimientos.UltimoCorreo);
        var enviado = Assert.Single(_correo.Enviados);
        Assert.Equal(Token, enviado.Token);   // el correo lleva el token; la BD, solo su hash
        Assert.Equal("Ana", enviado.Nombre);
    }

    [Fact]
    public async Task Un_correo_no_registrado_recibe_la_misma_respuesta_sin_enviar_nada()
    {
        var respuesta = await Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand("nadie@agro.test"));

        Assert.Equal(SolicitarRecuperacionHandler.Respuesta, respuesta.Mensaje);
        Assert.Empty(_correo.Enviados);
    }

    [Fact]
    public async Task Despues_de_3_solicitudes_en_una_hora_ya_no_envia_pero_responde_igual()
    {
        for (var i = 0; i < 4; i++)
        {
            var respuesta = await Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand(RecuperacionProcedimientosFalsos.Correo));
            Assert.Equal(SolicitarRecuperacionHandler.Respuesta, respuesta.Mensaje);
        }

        Assert.Equal(3, _correo.Enviados.Count);
    }

    [Fact]
    public async Task Un_correo_invalido_responde_400()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand("no-es-correo")));

        Assert.Null(_procedimientos.UltimoCorreo);   // no llega a la base de datos
    }

    // ----- Restablecer -----

    [Fact]
    public async Task Restablece_la_contrasena_con_un_token_vigente()
    {
        await Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand(RecuperacionProcedimientosFalsos.Correo));

        await Restablecer().EjecutarAsync(new RestablecerContrasenaCommand(Token, "NuevaSegura456"));

        Assert.Equal("hash:NuevaSegura456", _procedimientos.HashNuevo);   // se guarda el hash, nunca el texto
    }

    [Fact]
    public async Task El_mismo_token_no_sirve_dos_veces()
    {
        await Solicitar().EjecutarAsync(new SolicitarRecuperacionCommand(RecuperacionProcedimientosFalsos.Correo));
        await Restablecer().EjecutarAsync(new RestablecerContrasenaCommand(Token, "NuevaSegura456"));

        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Restablecer().EjecutarAsync(new RestablecerContrasenaCommand(Token, "OtraSegura789")));

        var campo = Assert.Single(error.Errors);
        Assert.Equal("token", campo.PropertyName);
        Assert.Equal(RestablecerContrasenaHandler.EnlaceInvalido, campo.ErrorMessage);
    }

    [Fact]
    public async Task Un_token_que_no_existe_responde_400()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Restablecer().EjecutarAsync(new RestablecerContrasenaCommand("inventado", "NuevaSegura456")));
    }

    [Fact]
    public void Sin_token_el_enlace_no_es_valido()
    {
        var error = Assert.Single(new RestablecerContrasenaValidator()
            .Validate(new RestablecerContrasenaCommand("", "NuevaSegura456")).Errors);

        Assert.Equal(RestablecerContrasenaHandler.EnlaceInvalido, error.ErrorMessage);
    }

    [Fact]
    public void La_contrasena_nueva_debe_tener_al_menos_8_caracteres()
    {
        var error = Assert.Single(new RestablecerContrasenaValidator()
            .Validate(new RestablecerContrasenaCommand(Token, "corta")).Errors);

        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", error.ErrorMessage);
    }
}