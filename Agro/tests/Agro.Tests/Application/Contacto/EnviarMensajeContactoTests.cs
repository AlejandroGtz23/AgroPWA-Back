using Agro.Application.Common.Exceptions;
using Agro.Application.Contacto.Commands.EnviarMensajeContacto;
using FluentValidation;

namespace Agro.Tests.Application.Contacto;

/// <summary>Pruebas del formulario de contacto (HU-28, criterios 1 a 3 y límite de envíos).</summary>
public class EnviarMensajeContactoTests
{
    private readonly ContactoProcedimientosFalsos _procedimientos = new();
    private readonly EnviarMensajeContactoValidator _validador = new();

    private EnviarMensajeContactoHandler Handler() => new(_procedimientos, _validador);

    private static EnviarMensajeContactoCommand Comando(string correo = "ana@correo.com",
        string mensaje = "Quiero más información.", string? asunto = "Duda") =>
        new("Ana López", correo, asunto, mensaje);

    // ----- Handler -----

    [Fact]
    public async Task Guarda_el_mensaje_con_los_datos_limpios()
    {
        await Handler().EjecutarAsync(new EnviarMensajeContactoCommand("  Ana López ", "Ana@Correo.com", "   ", " Hola "));

        var mensaje = Assert.Single(_procedimientos.Mensajes);
        Assert.Equal("Ana López", mensaje.Nombre);
        Assert.Equal("ana@correo.com", mensaje.Correo);
        Assert.Null(mensaje.Asunto);
        Assert.Equal("Hola", mensaje.Mensaje);
    }

    [Fact]
    public async Task El_cuarto_mensaje_en_una_hora_responde_429()
    {
        for (var i = 0; i < EnviarMensajeContactoHandler.MaximoPorHora; i++)
            await Handler().EjecutarAsync(Comando());

        var error = await Assert.ThrowsAsync<DemasiadosIntentosException>(() => Handler().EjecutarAsync(Comando()));

        Assert.Equal(EnviarMensajeContactoHandler.LimiteExcedido, error.Message);
        Assert.Equal(3, _procedimientos.Mensajes.Count);
    }

    [Fact]
    public async Task Con_datos_invalidos_no_guarda_nada()
    {
        await Assert.ThrowsAsync<ValidationException>(() => Handler().EjecutarAsync(Comando(correo: "")));

        Assert.Empty(_procedimientos.Mensajes);
    }

    // ----- Validador -----

    [Fact]
    public void Acepta_un_mensaje_valido_con_o_sin_asunto()
    {
        Assert.True(_validador.Validate(Comando()).IsValid);
        Assert.True(_validador.Validate(Comando(asunto: null)).IsValid);
    }

    [Fact]
    public void Marca_los_campos_obligatorios_vacios()
    {
        var resultado = _validador.Validate(new EnviarMensajeContactoCommand("", "", null, ""));

        Assert.Equal(["Correo", "Mensaje", "Nombre"], resultado.Errors.Select(e => e.PropertyName).Order());
    }

    [Theory]
    [InlineData("ana")]
    [InlineData("ana@correo")]              // sin punto: lo rechazaría el CHECK de la tabla
    [InlineData("ana lopez@correo.com")]
    public void Rechaza_un_correo_invalido(string correo)
    {
        var error = Assert.Single(_validador.Validate(Comando(correo: correo)).Errors);
        Assert.Equal("Escribe un correo válido (ej. nombre@correo.com).", error.ErrorMessage);
    }

    [Fact]
    public void El_mensaje_admite_hasta_1000_caracteres()
    {
        Assert.True(_validador.Validate(Comando(mensaje: new string('a', 1000))).IsValid);

        var error = Assert.Single(_validador.Validate(Comando(mensaje: new string('a', 1001))).Errors);
        Assert.Equal("El mensaje admite hasta 1000 caracteres.", error.ErrorMessage);
    }
}