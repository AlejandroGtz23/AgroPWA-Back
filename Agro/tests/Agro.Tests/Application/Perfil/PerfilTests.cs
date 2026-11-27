using Agro.Application.Common.Exceptions;
using Agro.Application.Perfil.Commands.ActualizarPerfil;
using Agro.Application.Perfil.Commands.CambiarContrasena;
using Agro.Application.Perfil.Queries.ObtenerPerfil;
using Agro.Tests.Application.Auth;
using FluentValidation;

namespace Agro.Tests.Application.Perfil;

/// <summary>Pruebas del perfil (HU-26, criterios 1 a 4).</summary>
public class PerfilTests
{
    private const int Usuario = PerfilProcedimientosFalsos.Usuario;
    private const int OtroUsuario = 99;
    private const string Jti = "jti-de-esta-sesion";

    private readonly PerfilProcedimientosFalsos _procedimientos = new();

    private ActualizarPerfilHandler Actualizar() => new(_procedimientos, new ActualizarPerfilValidator());
    private CambiarContrasenaHandler Cambiar() => new(_procedimientos, new HashFalso(), new CambiarContrasenaValidator());

    private static ActualizarPerfilCommand Datos(string nombre = "Victor", string? telefono = "7711234567") =>
        new(nombre, "Jose", null, telefono, "Tula");

    // ----- Consultar -----

    [Fact]
    public async Task Obtiene_el_perfil_del_usuario()
    {
        var perfil = await new ObtenerPerfilHandler(_procedimientos).EjecutarAsync(Usuario);

        Assert.Equal("ana@agro.test", perfil.Correo);
        Assert.Equal("productor", perfil.Rol);
        Assert.False(perfil.CorreoVerificado);
    }

    [Fact]
    public async Task Un_usuario_inexistente_responde_404()
    {
        var error = await Assert.ThrowsAsync<NoEncontradoException>(
            () => new ObtenerPerfilHandler(_procedimientos).EjecutarAsync(OtroUsuario));

        Assert.Equal(ObtenerPerfilHandler.NoEncontrado, error.Message);
    }

    // ----- Actualizar -----

    [Fact]
    public async Task Actualiza_los_datos_y_el_correo_no_cambia()
    {
        var perfil = await Actualizar().EjecutarAsync(Usuario, Datos());

        Assert.Equal("Victor", perfil.Nombre);
        Assert.Equal("7711234567", perfil.Telefono);
        Assert.Equal("ana@agro.test", perfil.Correo);   // el command ni siquiera tiene correo
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void El_telefono_es_opcional(string? telefono)
    {
        Assert.True(new ActualizarPerfilValidator().Validate(Datos(telefono: telefono)).IsValid);
    }

    [Fact]
    public async Task Un_telefono_que_no_tiene_10_digitos_responde_400()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Actualizar().EjecutarAsync(Usuario, Datos(telefono: "123")));

        Assert.Equal("El teléfono debe tener 10 dígitos.", Assert.Single(error.Errors).ErrorMessage);
    }

    [Fact]
    public async Task Un_nombre_de_una_letra_responde_400()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Actualizar().EjecutarAsync(Usuario, Datos(nombre: "V")));

        Assert.Equal("El nombre debe tener al menos 2 letras.", Assert.Single(error.Errors).ErrorMessage);
    }

    [Fact]
    public async Task Actualizar_un_usuario_inexistente_responde_404()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => Actualizar().EjecutarAsync(OtroUsuario, Datos()));
    }

    // ----- Cambiar contraseña -----

    [Fact]
    public async Task Cambia_la_contrasena_y_conserva_la_sesion_actual()
    {
        await Cambiar().EjecutarAsync(Usuario, Jti, new CambiarContrasenaCommand("Actual123", "NuevaSegura456"));

        Assert.Equal("hash:NuevaSegura456", _procedimientos.Hash);   // se guarda el hash, nunca el texto
        Assert.Equal(Jti, _procedimientos.JtiConservado);
    }

    [Fact]
    public async Task Con_la_contrasena_actual_incorrecta_responde_400_y_no_cambia_nada()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Cambiar().EjecutarAsync(Usuario, Jti, new CambiarContrasenaCommand("Equivocada1", "NuevaSegura456")));

        var campo = Assert.Single(error.Errors);
        Assert.Equal("contrasenaActual", campo.PropertyName);
        Assert.Equal(CambiarContrasenaHandler.ActualIncorrecta, campo.ErrorMessage);
        Assert.Equal("hash:Actual123", _procedimientos.Hash);
    }

    [Fact]
    public void La_nueva_debe_ser_diferente_a_la_actual()
    {
        var error = Assert.Single(new CambiarContrasenaValidator()
            .Validate(new CambiarContrasenaCommand("Actual123", "Actual123")).Errors);

        Assert.Equal("La contraseña nueva debe ser diferente a la actual.", error.ErrorMessage);
    }

    [Fact]
    public void La_nueva_debe_tener_al_menos_8_caracteres()
    {
        var error = Assert.Single(new CambiarContrasenaValidator()
            .Validate(new CambiarContrasenaCommand("Actual123", "corta")).Errors);

        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", error.ErrorMessage);
    }

    [Fact]
    public async Task Cambiar_la_contrasena_de_un_usuario_inexistente_responde_404()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() =>
            Cambiar().EjecutarAsync(OtroUsuario, Jti, new CambiarContrasenaCommand("Actual123", "NuevaSegura456")));
    }
}