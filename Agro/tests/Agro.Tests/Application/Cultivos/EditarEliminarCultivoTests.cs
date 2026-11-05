using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Commands.EditarCultivo;
using Agro.Application.Cultivos.Commands.EliminarCultivo;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using FluentValidation;

namespace Agro.Tests.Application.Cultivos;

/// <summary>Pruebas de editar cultivo (HU-17, criterios 1 y 2) y eliminar cultivo (HU-18, criterio 3).</summary>
public class EditarEliminarCultivoTests
{
    private const int Usuario = 1;
    private const int OtroUsuario = 2;
    private const int MiParcela = 10;
    private static readonly DateOnly Siembra = new(2026, 6, 12);

    // 28/oct/2026 15:00 en México (21:00 UTC)
    private static readonly RelojFijo Reloj = new(new DateTimeOffset(2026, 10, 28, 21, 0, 0, TimeSpan.Zero));

    private readonly CultivosProcedimientosFalsos _procedimientos = new();
    private readonly EditarCultivoValidator _validador = new(Reloj);

    public EditarEliminarCultivoTests() => _procedimientos.AgregarParcela(MiParcela, Usuario);

    private EditarCultivoHandler Editar() => new(_procedimientos, _validador);
    private EliminarCultivoHandler Eliminar() => new(_procedimientos);

    /// <summary>Registra un cultivo del usuario en el falso y devuelve su id.</summary>
    private async Task<int> CrearCultivoAsync() =>
        (await _procedimientos.RegistrarAsync(Usuario, null,
            new NuevoCultivo(MiParcela, 1, "Maíz criollo", null, Siembra, null))).IdCultivo;

    private static EditarCultivoCommand Comando(string estado = "activo", DateOnly? cosecha = null,
        string nombre = "Maíz editado", string? variedad = "Criollo") =>
        new(nombre, 1, variedad, Siembra, MiParcela, estado, cosecha, "Riego por goteo", null);

    // ----- Editar: handler -----

    [Fact]
    public async Task Edita_el_cultivo_y_devuelve_los_datos_nuevos()
    {
        var id = await CrearCultivoAsync();

        var cultivo = await Editar().EjecutarAsync(id, Usuario, Comando("finalizado", new DateOnly(2026, 10, 20)));

        Assert.Equal("Maíz editado", cultivo.Nombre);
        Assert.Equal("finalizado", cultivo.Estado);
        Assert.Equal(new DateOnly(2026, 10, 20), cultivo.FechaCosecha);
    }

    [Fact]
    public async Task Normaliza_el_estado_y_los_textos_antes_de_guardar()
    {
        var id = await CrearCultivoAsync();

        await Editar().EjecutarAsync(id, Usuario, Comando(" Perdido ", nombre: "  Maíz  ", variedad: "   "));

        var cambio = _procedimientos.UltimoCambio!;
        Assert.Equal("perdido", cambio.Estado);
        Assert.Equal("Maíz", cambio.Datos.Nombre);
        Assert.Null(cambio.Datos.Variedad);
    }

    [Fact]
    public async Task Editar_un_cultivo_de_otro_usuario_responde_404()
    {
        var id = await CrearCultivoAsync();

        var error = await Assert.ThrowsAsync<NoEncontradoException>(
            () => Editar().EjecutarAsync(id, OtroUsuario, Comando()));

        Assert.Equal(ObtenerDetalleCultivoHandler.NoEncontrado, error.Message);
    }

    [Fact]
    public async Task Con_datos_invalidos_no_llega_a_la_base_de_datos()
    {
        var id = await CrearCultivoAsync();

        await Assert.ThrowsAsync<ValidationException>(
            () => Editar().EjecutarAsync(id, Usuario, Comando("finalizado")));   // sin fecha de cosecha

        Assert.Null(_procedimientos.UltimoCambio);
    }

    // ----- Editar: validador -----

    [Fact]
    public void Acepta_los_tres_estados_validos()
    {
        Assert.True(_validador.Validate(Comando("activo")).IsValid);
        Assert.True(_validador.Validate(Comando("perdido")).IsValid);
        Assert.True(_validador.Validate(Comando("Finalizado", new DateOnly(2026, 10, 28))).IsValid);   // cosecha hoy
    }

    [Theory]
    [InlineData("")]
    [InlineData("cosechado")]
    public void Rechaza_un_estado_vacio_o_desconocido(string estado)
    {
        var error = Assert.Single(_validador.Validate(Comando(estado)).Errors);
        Assert.Equal("Estado", error.PropertyName);
    }

    [Fact]
    public void Finalizado_sin_fecha_de_cosecha_la_pide()
    {
        var error = Assert.Single(_validador.Validate(Comando("finalizado")).Errors);
        Assert.Equal("Captura la fecha de cosecha.", error.ErrorMessage);
    }

    [Fact]
    public void La_cosecha_no_puede_ser_antes_de_la_siembra()
    {
        var error = Assert.Single(_validador.Validate(Comando("finalizado", Siembra.AddDays(-1))).Errors);
        Assert.Equal("La fecha de cosecha no puede ser anterior a la de siembra.", error.ErrorMessage);
    }

    [Fact]
    public void La_cosecha_no_puede_ser_despues_de_hoy()
    {
        var error = Assert.Single(_validador.Validate(Comando("finalizado", new DateOnly(2026, 10, 29))).Errors);
        Assert.Equal("La fecha de cosecha no puede ser posterior a hoy.", error.ErrorMessage);
    }

    [Fact]
    public void Aplica_las_mismas_reglas_que_registrar()
    {
        var resultado = _validador.Validate(new EditarCultivoCommand("", null, null, null, null, "activo", null, null, null));

        Assert.Equal(["FechaSiembra", "IdParcela", "IdTipoCultivo", "Nombre"],
            resultado.Errors.Select(e => e.PropertyName).Order());
    }

    // ----- Eliminar -----

    [Fact]
    public async Task Elimina_el_cultivo_del_usuario()
    {
        var id = await CrearCultivoAsync();

        await Eliminar().EjecutarAsync(id, Usuario);

        Assert.Null(await _procedimientos.ObtenerDetalleAsync(id, Usuario));
    }

    [Fact]
    public async Task Eliminar_un_cultivo_de_otro_usuario_responde_404_y_no_lo_borra()
    {
        var id = await CrearCultivoAsync();

        var error = await Assert.ThrowsAsync<NoEncontradoException>(() => Eliminar().EjecutarAsync(id, OtroUsuario));

        Assert.Equal(ObtenerDetalleCultivoHandler.NoEncontrado, error.Message);
        Assert.NotNull(await _procedimientos.ObtenerDetalleAsync(id, Usuario));
    }
}