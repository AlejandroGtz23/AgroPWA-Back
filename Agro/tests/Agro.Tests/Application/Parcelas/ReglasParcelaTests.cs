using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Parcelas.Common;

namespace Agro.Tests.Application.Parcelas;

/// <summary>Reglas del formulario de parcela (HU-13, criterios 2 y 3). Coinciden con la tabla parcela.</summary>
public class ReglasParcelaTests
{
    private readonly RegistrarParcelaValidator _registrar = new();
    private readonly EditarParcelaValidator _editar = new();

    private static readonly RegistrarParcelaCommand Valida = new(null, "Parcela Norte", 1.5m, "San Marcos");

    [Fact]
    public void Una_parcela_correcta_es_valida()
    {
        Assert.True(_registrar.Validate(Valida).IsValid);
        Assert.True(_editar.Validate(new EditarParcelaCommand("Parcela Norte", 1.5m, null)).IsValid);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(999999.99)]
    [InlineData(1.50)]
    public void Superficies_en_el_limite_son_validas(double superficie)
    {
        Assert.True(_registrar.Validate(Valida with { SuperficieHa = (decimal)superficie }).IsValid);
    }

    [Fact]
    public void El_nombre_es_obligatorio_con_un_solo_mensaje()
    {
        var resultado = _registrar.Validate(Valida with { Nombre = "" });

        var error = Assert.Single(resultado.Errors);
        Assert.Equal("El nombre de la parcela es obligatorio.", error.ErrorMessage);
    }

    [Fact]
    public void El_nombre_que_no_llega_es_invalido_sin_lanzar_excepcion()
    {
        Assert.False(_registrar.Validate(Valida with { Nombre = null! }).IsValid);
    }

    [Fact]
    public void El_nombre_no_puede_pasar_de_80_caracteres()
    {
        AssertInvalido(Valida with { Nombre = new string('a', 81) }, nameof(RegistrarParcelaCommand.Nombre));
    }

    [Theory]
    [InlineData(0, "Ingresa un número válido.")]
    [InlineData(-5, "Ingresa un número válido.")]
    [InlineData(1000000, "La superficie no puede ser mayor que 999,999.99 ha.")]
    [InlineData(1.555, "La superficie puede tener máximo 2 decimales.")]
    public void Superficie_invalida(double superficie, string mensaje)
    {
        var resultado = _registrar.Validate(Valida with { SuperficieHa = (decimal)superficie });

        var error = Assert.Single(resultado.Errors);   // Cascade.Stop: un solo mensaje
        Assert.Equal(mensaje, error.ErrorMessage);
    }

    [Fact]
    public void La_ubicacion_es_opcional_pero_maximo_150_caracteres()
    {
        Assert.True(_registrar.Validate(Valida with { UbicacionReferencia = null }).IsValid);
        AssertInvalido(Valida with { UbicacionReferencia = new string('a', 151) }, nameof(RegistrarParcelaCommand.UbicacionReferencia));
    }

    [Fact]
    public void Editar_aplica_las_mismas_reglas()
    {
        var resultado = _editar.Validate(new EditarParcelaCommand("", 0m, new string('a', 151)));

        Assert.Equal(["Nombre", "SuperficieHa", "UbicacionReferencia"],
            resultado.Errors.Select(e => e.PropertyName).Order());
    }

    [Fact]
    public void Normalizar_quita_espacios_y_convierte_la_ubicacion_vacia_en_null()
    {
        var datos = ReglasParcela.Normalizar("  Parcela Norte ", 2m, "  ");

        Assert.Equal("Parcela Norte", datos.Nombre);
        Assert.Equal(2m, datos.SuperficieHa);
        Assert.Null(datos.UbicacionReferencia);
        Assert.Equal("San Marcos", ReglasParcela.Normalizar("x", 1m, " San Marcos ").UbicacionReferencia);
    }

    private void AssertInvalido(RegistrarParcelaCommand comando, string campo)
    {
        var resultado = _registrar.Validate(comando);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == campo);
    }
}
