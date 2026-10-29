using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using FluentValidation;

namespace Agro.Tests.Application.Cultivos;

/// <summary>Pruebas de registrar cultivo: handler y validador (HU-14, criterios 1, 2 y 4).</summary>
public class RegistrarCultivoTests
{
    private const int Usuario = 1;
    private const int OtroUsuario = 2;
    private const int MiParcela = 10;
    private const int ParcelaAjena = 20;
    private static readonly Guid IdLocal = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    // 28/oct/2026 15:00 en México (21:00 UTC)
    private static readonly RelojFijo Reloj = new(new DateTimeOffset(2026, 10, 28, 21, 0, 0, TimeSpan.Zero));

    private readonly CultivosProcedimientosFalsos _procedimientos = new();
    private readonly RegistrarCultivoValidator _validador = new(Reloj);

    public RegistrarCultivoTests()
    {
        _procedimientos.AgregarParcela(MiParcela, Usuario);
        _procedimientos.AgregarParcela(ParcelaAjena, OtroUsuario);
    }

    private RegistrarCultivoHandler Handler() => new(_procedimientos, _validador);

    private static RegistrarCultivoCommand Comando(
        string nombre = "Maíz criollo", int? idTipo = 1, int? idParcela = MiParcela,
        DateOnly? siembra = null, Guid? idLocal = null, string? variedad = "Criollo", string? observaciones = "Riego por goteo") =>
        new(idLocal, nombre, idTipo, variedad, siembra ?? new DateOnly(2026, 6, 12), idParcela, observaciones);

    // ----- Handler -----

    [Fact]
    public async Task Registra_el_cultivo_activo_y_lo_devuelve()
    {
        var (cultivo, creado) = await Handler().EjecutarAsync(Usuario, Comando(idLocal: IdLocal));

        Assert.True(creado);
        Assert.Equal("Maíz criollo", cultivo.Nombre);
        Assert.Equal("Maíz", cultivo.Tipo);
        Assert.Equal(MiParcela, cultivo.IdParcela);
        Assert.Equal("activo", cultivo.Estado);
        Assert.Equal(IdLocal, cultivo.IdLocal);
    }

    [Fact]
    public async Task Con_el_mismo_idLocal_devuelve_el_existente_sin_duplicar()
    {
        var (primero, _) = await Handler().EjecutarAsync(Usuario, Comando(idLocal: IdLocal));

        var (segundo, creado) = await Handler().EjecutarAsync(Usuario, Comando(idLocal: IdLocal));

        Assert.False(creado);   // el controlador responde 200 en lugar de 201
        Assert.Equal(primero.Id, segundo.Id);
    }

    [Fact]
    public async Task Quita_espacios_y_guarda_variedad_y_observaciones_vacias_como_null()
    {
        await Handler().EjecutarAsync(Usuario, Comando(nombre: "  Frijol negro ", idTipo: 2, variedad: "  ", observaciones: ""));

        var datos = _procedimientos.UltimosDatos!;
        Assert.Equal("Frijol negro", datos.Nombre);
        Assert.Null(datos.Variedad);
        Assert.Null(datos.Observaciones);
        Assert.Equal(2, datos.IdTipoCultivo);
    }

    [Fact]
    public async Task En_una_parcela_de_otro_usuario_responde_400_en_idParcela()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Handler().EjecutarAsync(Usuario, Comando(idParcela: ParcelaAjena)));

        Assert.Contains(error.Errors, e => e.PropertyName == "idParcela");
    }

    [Fact]
    public async Task Con_un_tipo_inexistente_responde_400_en_idTipoCultivo()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Handler().EjecutarAsync(Usuario, Comando(idTipo: 999)));

        Assert.Contains(error.Errors, e => e.PropertyName == "idTipoCultivo");
    }

    [Fact]
    public async Task Con_datos_invalidos_responde_400_sin_llamar_al_procedimiento()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Handler().EjecutarAsync(Usuario, Comando(nombre: "", idTipo: null)));

        Assert.Equal(0, _procedimientos.LlamadasRegistrar);
    }

    // ----- Validador -----

    [Fact]
    public void Un_cultivo_correcto_es_valido_y_los_opcionales_pueden_faltar()
    {
        Assert.True(_validador.Validate(Comando()).IsValid);
        Assert.True(_validador.Validate(Comando(variedad: null, observaciones: null)).IsValid);
    }

    [Fact]
    public void Sin_datos_marca_los_cuatro_obligatorios()
    {
        var resultado = _validador.Validate(new RegistrarCultivoCommand(null, "", null, null, null, null, null));

        Assert.Equal(["FechaSiembra", "IdParcela", "IdTipoCultivo", "Nombre"],
            resultado.Errors.Select(e => e.PropertyName).Order());
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "El nombre del cultivo es obligatorio.");
    }

    [Fact]
    public void Una_fecha_de_siembra_posterior_a_hoy_no_es_valida()
    {
        var resultado = _validador.Validate(Comando(siembra: new DateOnly(2026, 10, 29)));

        var error = Assert.Single(resultado.Errors);
        Assert.Equal("La fecha de siembra no puede ser posterior a hoy.", error.ErrorMessage);
    }

    [Fact]
    public void Hoy_se_cuenta_en_hora_de_Mexico_aunque_en_UTC_ya_sea_manana()
    {
        // 28/oct 21:30 en México = 29/oct 03:30 UTC: el 29 todavía es «mañana» para el productor
        var validador = new RegistrarCultivoValidator(new RelojFijo(new DateTimeOffset(2026, 10, 29, 3, 30, 0, TimeSpan.Zero)));

        Assert.True(validador.Validate(Comando(siembra: new DateOnly(2026, 10, 28))).IsValid);
        Assert.False(validador.Validate(Comando(siembra: new DateOnly(2026, 10, 29))).IsValid);
    }

    [Fact]
    public void Una_fecha_anterior_al_2000_no_es_valida()
    {
        var error = Assert.Single(_validador.Validate(Comando(siembra: new DateOnly(1999, 12, 31))).Errors);
        Assert.Equal("La fecha de siembra no es válida.", error.ErrorMessage);
    }

    [Theory]
    [InlineData(81, 0, 0, "Nombre")]
    [InlineData(0, 81, 0, "Variedad")]
    [InlineData(0, 0, 301, "Observaciones")]
    public void Respeta_los_largos_maximos(int nombre, int variedad, int observaciones, string campo)
    {
        var comando = Comando(
            nombre: nombre > 0 ? new string('a', nombre) : "Maíz",
            variedad: variedad > 0 ? new string('a', variedad) : null,
            observaciones: observaciones > 0 ? new string('a', observaciones) : null);

        var error = Assert.Single(_validador.Validate(comando).Errors);
        Assert.Equal(campo, error.PropertyName);
    }
}
