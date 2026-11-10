using Agro.Application.Avances.Commands.RegistrarAvance;
using Agro.Application.Catalogos.Queries.ObtenerTiposActividad;
using Agro.Application.Common.Exceptions;
using Agro.Tests.Application.Cultivos;
using FluentValidation;

namespace Agro.Tests.Application.Avances;

/// <summary>Pruebas de registrar avance (HU-19, criterios 1, 3 y 4) y del catálogo de tipos de actividad.</summary>
public class RegistrarAvanceTests
{
    private const int Usuario = 1;
    private const int OtroUsuario = 2;
    private const int MiCultivo = 10;
    private const int CultivoFinalizado = 11;
    private const int Riego = 1;
    private const int Fertilizacion = 2;
    private const int TipoAvance = 3;
    private static readonly Guid IdLocal = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    // 10/nov/2026 12:00 en México (18:00 UTC)
    private static readonly DateTimeOffset Ahora = new(2026, 11, 10, 18, 0, 0, TimeSpan.Zero);
    private static readonly RelojFijo Reloj = new(Ahora);

    private readonly AvancesProcedimientosFalsos _procedimientos = new();
    private readonly RegistrarAvanceValidator _validador = new(Reloj);

    public RegistrarAvanceTests()
    {
        _procedimientos.AgregarCultivo(MiCultivo, Usuario);
        _procedimientos.AgregarCultivo(CultivoFinalizado, Usuario, activo: false);
    }

    private RegistrarAvanceHandler Handler() => new(_procedimientos, _validador);

    private static RegistrarAvanceCommand RiegoDe40Min(Guid? idLocal = null, string descripcion = "Riego de la mañana",
        DateTimeOffset? fecha = null, int? duracion = 40) =>
        new(idLocal, Riego, fecha ?? Ahora.AddHours(-2), descripcion, duracion, "Goteo", null, null, null);

    private static RegistrarAvanceCommand FertilizacionCon(decimal? cantidad) =>
    new(null, Fertilizacion, Ahora.AddHours(-1), "Abono", null, null, "Urea", cantidad, "kg");
    // ----- Handler -----

    [Fact]
    public async Task Registra_el_avance_y_lo_devuelve()
    {
        var (avance, creado) = await Handler().EjecutarAsync(MiCultivo, Usuario, RiegoDe40Min(IdLocal));

        Assert.True(creado);
        Assert.Equal("Riego", avance.Tipo);
        Assert.Equal(MiCultivo, avance.IdCultivo);
        Assert.Equal(40, avance.DuracionMin);
        Assert.Equal(IdLocal, avance.IdLocal);
    }

    [Fact]
    public async Task Con_el_mismo_idLocal_devuelve_el_existente_sin_duplicar()
    {
        var (primero, _) = await Handler().EjecutarAsync(MiCultivo, Usuario, RiegoDe40Min(IdLocal));

        var (segundo, creado) = await Handler().EjecutarAsync(MiCultivo, Usuario, RiegoDe40Min(IdLocal));

        Assert.False(creado);   // el controlador responde 200 en lugar de 201
        Assert.Equal(primero.Id, segundo.Id);
    }

    [Fact]
    public async Task Quita_espacios_y_convierte_los_textos_vacios_en_null()
    {
        await Handler().EjecutarAsync(MiCultivo, Usuario,
            new RegistrarAvanceCommand(null, Riego, Ahora.AddHours(-1), "  Riego  ", 40, "  Goteo ", "   ", null, " "));

        var datos = _procedimientos.UltimosDatos!;
        Assert.Equal("Riego", datos.Descripcion);
        Assert.Equal("Goteo", datos.MetodoRiego);
        Assert.Null(datos.Producto);
        Assert.Null(datos.Unidad);
    }

    [Fact]
    public async Task En_un_cultivo_de_otro_usuario_responde_404()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => Handler().EjecutarAsync(MiCultivo, OtroUsuario, RiegoDe40Min()));
    }

    [Fact]
    public async Task En_un_cultivo_finalizado_responde_409()
    {
        var error = await Assert.ThrowsAsync<ConflictoException>(
            () => Handler().EjecutarAsync(CultivoFinalizado, Usuario, RiegoDe40Min()));

        Assert.Equal("Solo se pueden registrar avances en cultivos activos.", error.Message);
    }

    [Fact]
    public async Task Con_un_tipo_de_actividad_inexistente_responde_400_en_ese_campo()
    {
        var comando = RiegoDe40Min() with { IdTipoActividad = 999 };

        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Handler().EjecutarAsync(MiCultivo, Usuario, comando));

        Assert.Contains(error.Errors, e => e.PropertyName == "idTipoActividad");
    }

    [Fact]
    public async Task Con_datos_invalidos_no_llega_a_la_base_de_datos()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Handler().EjecutarAsync(MiCultivo, Usuario, RiegoDe40Min(descripcion: "")));

        Assert.Equal(0, _procedimientos.LlamadasRegistrar);
    }

    [Fact]
    public async Task El_catalogo_devuelve_los_tipos_de_actividad()
    {
        var tipos = await new ObtenerTiposActividadHandler(_procedimientos).EjecutarAsync();

        Assert.Equal(["Riego", "Fertilización", "Avance"], tipos.Select(t => t.Nombre));
    }

    // ----- Validador -----

    [Fact]
    public void Acepta_un_riego_y_un_avance_sin_campos_extra()
    {
        Assert.True(_validador.Validate(RiegoDe40Min()).IsValid);
        Assert.True(_validador.Validate(
            new RegistrarAvanceCommand(null, TipoAvance, Ahora.AddDays(-1), "Ya salió la mazorca", null, null, null, null, null)).IsValid);
    }

    [Fact]
    public void Marca_los_campos_obligatorios()
    {
        var resultado = _validador.Validate(new RegistrarAvanceCommand(null, null, null, "", null, null, null, null, null));

        Assert.Equal(["Descripcion", "FechaActividad", "IdTipoActividad"],
            resultado.Errors.Select(e => e.PropertyName).Order());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void La_duracion_debe_ser_mayor_que_0(int duracion)
    {
        var error = Assert.Single(_validador.Validate(RiegoDe40Min(duracion: duracion)).Errors);
        Assert.Equal("La duración debe ser mayor que 0 minutos.", error.ErrorMessage);
    }

    [Fact]
    public void La_fecha_no_puede_estar_en_el_futuro_salvo_5_minutos_de_tolerancia()
    {
        Assert.True(_validador.Validate(RiegoDe40Min(fecha: Ahora.AddMinutes(4))).IsValid);

        var error = Assert.Single(_validador.Validate(RiegoDe40Min(fecha: Ahora.AddMinutes(10))).Errors);
        Assert.Equal("La fecha no puede estar en el futuro.", error.ErrorMessage);
    }

    [Fact]
    public void La_cantidad_debe_ser_mayor_que_0_y_con_maximo_2_decimales()
    {
        Assert.True(_validador.Validate(FertilizacionCon(2.5m)).IsValid);
        Assert.Equal("La cantidad debe ser mayor que 0.",
            Assert.Single(_validador.Validate(FertilizacionCon(0m)).Errors).ErrorMessage);
        Assert.Equal("La cantidad admite máximo 2 decimales.",
            Assert.Single(_validador.Validate(FertilizacionCon(2.555m)).Errors).ErrorMessage);
    }

    [Fact]
    public void La_descripcion_admite_hasta_500_caracteres()
    {
        Assert.True(_validador.Validate(RiegoDe40Min(descripcion: new string('a', 500))).IsValid);
        Assert.False(_validador.Validate(RiegoDe40Min(descripcion: new string('a', 501))).IsValid);
    }
}