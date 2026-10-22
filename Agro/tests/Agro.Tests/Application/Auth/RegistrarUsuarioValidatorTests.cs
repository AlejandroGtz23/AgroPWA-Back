using Agro.Application.Auth.Commands.RegistrarUsuario;

namespace Agro.Tests.Application.Auth;

/// <summary>Reglas del formulario de registro (HU-09, criterio 1). Coinciden con los CHECK de la tabla usuario.</summary>
public class RegistrarUsuarioValidatorTests
{
    private readonly RegistrarUsuarioValidator _validador = new();

    private static readonly RegistrarUsuarioCommand Valido = new(
        "Victor", "Jose", "López", "victor@correo.com", "7711234567",
        new DateOnly(2000, 5, 10), "Secreta123", true);

    [Fact]
    public void Un_registro_correcto_es_valido()
    {
        Assert.True(_validador.Validate(Valido).IsValid);
    }

    [Fact]
    public void Apellido_materno_y_telefono_son_opcionales()
    {
        var comando = Valido with { ApellidoMaterno = null, Telefono = null };

        Assert.True(_validador.Validate(comando).IsValid);
    }

    [Theory]
    [InlineData("", nameof(RegistrarUsuarioCommand.Nombre))]
    [InlineData("V", nameof(RegistrarUsuarioCommand.Nombre))]
    public void Nombre_obligatorio_y_de_al_menos_2_letras(string nombre, string campo)
    {
        AssertInvalido(Valido with { Nombre = nombre }, campo);
    }

    [Fact]
    public void Nombre_o_apellido_que_no_llegan_son_invalidos_sin_lanzar_excepcion()
    {
        // Si el JSON no trae el campo llega null: antes .Trim() lanzaba NullReferenceException (500)
        AssertInvalido(Valido with { Nombre = null! }, nameof(RegistrarUsuarioCommand.Nombre));
        AssertInvalido(Valido with { ApellidoPaterno = null! }, nameof(RegistrarUsuarioCommand.ApellidoPaterno));
    }

    [Fact]
    public void Un_campo_vacio_muestra_un_solo_mensaje()
    {
        var resultado = _validador.Validate(Valido with { Nombre = "" });

        Assert.Single(resultado.Errors, e => e.PropertyName == nameof(RegistrarUsuarioCommand.Nombre));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" J ")]
    public void Apellido_paterno_obligatorio_y_de_al_menos_2_letras(string apellido)
    {
        AssertInvalido(Valido with { ApellidoPaterno = apellido }, nameof(RegistrarUsuarioCommand.ApellidoPaterno));
    }

    [Fact]
    public void Apellido_materno_no_puede_pasar_de_60_caracteres()
    {
        AssertInvalido(Valido with { ApellidoMaterno = new string('a', 61) }, nameof(RegistrarUsuarioCommand.ApellidoMaterno));
    }

    [Theory]
    [InlineData("")]
    [InlineData("victor")]
    [InlineData("victor.correo.com")]
    public void Correo_obligatorio_y_con_formato_valido(string correo)
    {
        AssertInvalido(Valido with { Correo = correo }, nameof(RegistrarUsuarioCommand.Correo));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("77112345678")]
    [InlineData("77-1123-456")]
    public void Telefono_debe_tener_10_digitos(string telefono)
    {
        AssertInvalido(Valido with { Telefono = telefono }, nameof(RegistrarUsuarioCommand.Telefono));
    }

    [Fact]
    public void Fecha_de_nacimiento_antes_de_1900_no_es_valida()
    {
        AssertInvalido(Valido with { FechaNacimiento = new DateOnly(1899, 12, 31) }, nameof(RegistrarUsuarioCommand.FechaNacimiento));
    }

    [Fact]
    public void Fecha_de_nacimiento_no_puede_ser_futura()
    {
        var manana = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        AssertInvalido(Valido with { FechaNacimiento = manana }, nameof(RegistrarUsuarioCommand.FechaNacimiento));
    }

    [Theory]
    [InlineData("")]
    [InlineData("corta1")]
    public void Contrasena_obligatoria_y_de_al_menos_8_caracteres(string contrasena)
    {
        AssertInvalido(Valido with { Contrasena = contrasena }, nameof(RegistrarUsuarioCommand.Contrasena));
    }

    [Fact]
    public void Contrasena_no_puede_pasar_de_72_caracteres_limite_de_BCrypt()
    {
        AssertInvalido(Valido with { Contrasena = new string('a', 73) }, nameof(RegistrarUsuarioCommand.Contrasena));
    }

    [Fact]
    public void Debe_aceptar_los_terminos()
    {
        AssertInvalido(Valido with { AceptaTerminos = false }, nameof(RegistrarUsuarioCommand.AceptaTerminos));
    }

    private void AssertInvalido(RegistrarUsuarioCommand comando, string campo)
    {
        var resultado = _validador.Validate(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }
}
