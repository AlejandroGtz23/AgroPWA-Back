using Agro.Application.Auth.Commands.RegistrarUsuario;
using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using FluentValidation;

namespace Agro.Tests.Application.Auth;

/// <summary>Pruebas del registro de usuario (HU-09).</summary>
public class RegistrarUsuarioHandlerTests
{
    private readonly ProcedimientosFalsos _procedimientos = new();

    private RegistrarUsuarioHandler CrearHandler() => new(
        _procedimientos, new HashFalso(),
        new CreadorSesion(new GeneradorJwtFalso(), _procedimientos, TimeProvider.System),
        new RegistrarUsuarioValidator());

    private static RegistrarUsuarioCommand Comando(string? apellidoMaterno = "López", string? telefono = "7711234567")
        => new("  Victor ", "Jose", apellidoMaterno, " Victor@Correo.COM ", telefono,
               new DateOnly(2000, 5, 10), "Secreta123", true);

    [Fact]
    public async Task Crea_la_cuenta_y_devuelve_la_sesion_con_rol_productor()
    {
        var sesion = await CrearHandler().EjecutarAsync(Comando(), "1.1.1.1", "xunit");

        Assert.Equal("token-acceso", sesion.AccessToken);
        Assert.Equal(7, sesion.Usuario.Id);
        Assert.Equal("productor", sesion.Usuario.Rol);
        Assert.Equal(7, _procedimientos.SesionCreada!.IdUsuario);
    }

    [Fact]
    public async Task Guarda_la_contrasena_como_hash_nunca_en_texto_plano()
    {
        await CrearHandler().EjecutarAsync(Comando(), null, null);

        Assert.NotEqual("Secreta123", _procedimientos.UsuarioRegistrado!.ContrasenaHash);
        Assert.Equal("hash:Secreta123", _procedimientos.UsuarioRegistrado.ContrasenaHash);
    }

    [Fact]
    public async Task Limpia_espacios_y_guarda_el_correo_en_minusculas()
    {
        await CrearHandler().EjecutarAsync(Comando(), null, null);

        var datos = _procedimientos.UsuarioRegistrado!;
        Assert.Equal("Victor", datos.Nombre);
        Assert.Equal("victor@correo.com", datos.Correo);
        Assert.Equal("López", datos.ApellidoMaterno);
        Assert.Equal("7711234567", datos.Telefono);
    }

    [Fact]
    public async Task Apellido_materno_y_telefono_vacios_se_guardan_como_null()
    {
        await CrearHandler().EjecutarAsync(Comando(apellidoMaterno: " ", telefono: ""), null, null);

        Assert.Null(_procedimientos.UsuarioRegistrado!.ApellidoMaterno);
        Assert.Null(_procedimientos.UsuarioRegistrado.Telefono);
    }

    [Fact]
    public async Task Si_el_correo_ya_existe_lanza_409()
    {
        _procedimientos.CorreoDuplicado = true;

        var error = await Assert.ThrowsAsync<ConflictoException>(
            () => CrearHandler().EjecutarAsync(Comando(), null, null));

        Assert.Contains("correo", error.Message);
        Assert.Null(_procedimientos.SesionCreada);
    }

    [Fact]
    public async Task Con_datos_invalidos_lanza_error_de_validacion_y_no_registra()
    {
        var comando = Comando() with { AceptaTerminos = false };

        await Assert.ThrowsAsync<ValidationException>(() => CrearHandler().EjecutarAsync(comando, null, null));

        Assert.Null(_procedimientos.UsuarioRegistrado);
    }
}
