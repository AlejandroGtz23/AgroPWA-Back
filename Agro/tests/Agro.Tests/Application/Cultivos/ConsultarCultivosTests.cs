using Agro.Application.Catalogos.Queries.ObtenerTiposCultivo;
using Agro.Application.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using Agro.Application.Cultivos.Common;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using Agro.Application.Cultivos.Queries.ObtenerMisCultivos;
using FluentValidation;

namespace Agro.Tests.Application.Cultivos;

/// <summary>Pruebas del catálogo (HU-14), «Mis cultivos» (HU-15) y el detalle (HU-16).</summary>
public class ConsultarCultivosTests
{
    private const int Usuario = 1;
    private const int OtroUsuario = 2;

    private readonly CultivosProcedimientosFalsos _procedimientos = new();

    private ObtenerMisCultivosHandler MisCultivos() => new(_procedimientos, new ObtenerMisCultivosValidator());
    private ObtenerDetalleCultivoHandler Detalle() => new(_procedimientos);

    /// <summary>Registra un cultivo con el handler real, para que el falso tenga datos.</summary>
    private async Task<int> RegistrarAsync(int idUsuario, string nombre)
    {
        var parcela = idUsuario * 10;
        _procedimientos.AgregarParcela(parcela, idUsuario);
        var handler = new RegistrarCultivoHandler(_procedimientos, new RegistrarCultivoValidator(TimeProvider.System));
        var (cultivo, _) = await handler.EjecutarAsync(idUsuario,
            new RegistrarCultivoCommand(null, nombre, 1, null, new DateOnly(2026, 6, 12), parcela, null));
        return cultivo.Id;
    }

    // ----- Catálogo -----

    [Fact]
    public async Task El_catalogo_devuelve_los_tipos_de_cultivo()
    {
        var tipos = await new ObtenerTiposCultivoHandler(_procedimientos).EjecutarAsync();

        Assert.Equal(["Maíz", "Frijol", "Calabaza"], tipos.Select(t => t.Nombre));
    }

    // ----- Mis cultivos -----

    [Fact]
    public async Task Mis_cultivos_solo_trae_los_del_usuario_del_mas_reciente_al_mas_antiguo()
    {
        await RegistrarAsync(Usuario, "Maíz criollo");
        await RegistrarAsync(Usuario, "Frijol negro");
        await RegistrarAsync(OtroUsuario, "Ajeno");

        var pagina = await MisCultivos().EjecutarAsync(Usuario, new ObtenerMisCultivosQuery(null, null, null));

        Assert.Equal(2, pagina.Total);
        Assert.Equal(["Frijol negro", "Maíz criollo"], pagina.Elementos.Select(c => c.Nombre));
    }

    [Fact]
    public async Task Sin_cultivos_devuelve_una_lista_vacia()
    {
        var pagina = await MisCultivos().EjecutarAsync(Usuario, new ObtenerMisCultivosQuery(null, null, null));

        Assert.Empty(pagina.Elementos);
        Assert.Equal(0, pagina.Total);
    }

    [Fact]
    public async Task Normaliza_los_filtros_antes_de_consultar()
    {
        await MisCultivos().EjecutarAsync(Usuario,
            new ObtenerMisCultivosQuery("  maiz ", " Activo ", 2, Pagina: 3, Tamano: 10));

        var filtro = _procedimientos.UltimoFiltro!;
        Assert.Equal("maiz", filtro.Buscar);
        Assert.Equal("activo", filtro.Estado);
        Assert.Equal(2, filtro.IdTipoCultivo);
        Assert.Equal(3, filtro.Pagina);
        Assert.Equal(10, filtro.Tamano);
    }

    [Fact]
    public async Task Una_busqueda_o_un_estado_vacios_no_filtran()
    {
        await MisCultivos().EjecutarAsync(Usuario, new ObtenerMisCultivosQuery("   ", "", null));

        Assert.Null(_procedimientos.UltimoFiltro!.Buscar);
        Assert.Null(_procedimientos.UltimoFiltro.Estado);
        Assert.Equal(1, _procedimientos.UltimoFiltro.Pagina);    // valores por defecto
        Assert.Equal(20, _procedimientos.UltimoFiltro.Tamano);
    }

    [Theory]
    [InlineData("cosechado", 1, 20, "Estado")]
    [InlineData(null, 0, 20, "Pagina")]
    [InlineData(null, 1, 0, "Tamano")]
    [InlineData(null, 1, 51, "Tamano")]
    public async Task Parametros_invalidos_responden_400_sin_consultar(string? estado, int pagina, int tamano, string campo)
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() => MisCultivos().EjecutarAsync(Usuario,
            new ObtenerMisCultivosQuery(null, estado, null, pagina, tamano)));

        Assert.Contains(error.Errors, e => e.PropertyName == campo);
        Assert.Null(_procedimientos.UltimoFiltro);
    }

    [Fact]
    public void Una_busqueda_de_mas_de_80_caracteres_no_es_valida()
    {
        var resultado = new ObtenerMisCultivosValidator().Validate(new ObtenerMisCultivosQuery(new string('a', 81), null, null));

        Assert.Equal("Buscar", Assert.Single(resultado.Errors).PropertyName);
    }

    // ----- Detalle -----

    [Fact]
    public async Task El_dueno_ve_el_detalle_con_avances_fotos_y_totales()
    {
        var id = await RegistrarAsync(Usuario, "Maíz criollo");
        _procedimientos.Avances.Add(new AvanceResumenDto(1, "Riego", DateTime.UtcNow, "Riego por goteo", Fotos: 1));
        _procedimientos.Fotografias.Add(new FotografiaDto(1, "https://x/foto.jpg", DateTime.UtcNow, EsPortada: true, IdAvance: 1));

        var detalle = await Detalle().EjecutarAsync(id, Usuario);

        Assert.Equal("Maíz criollo", detalle.Cultivo.Nombre);
        Assert.Equal("Riego", Assert.Single(detalle.Avances).Tipo);
        Assert.Single(detalle.Fotografias);
        Assert.Equal(new TotalesCultivoDto(1, 1), detalle.Totales);
    }

    [Fact]
    public async Task Un_cultivo_sin_avances_devuelve_listas_vacias_y_totales_en_cero()
    {
        var id = await RegistrarAsync(Usuario, "Frijol");

        var detalle = await Detalle().EjecutarAsync(id, Usuario);

        Assert.Empty(detalle.Avances);
        Assert.Empty(detalle.Fotografias);
        Assert.Equal(new TotalesCultivoDto(0, 0), detalle.Totales);
    }

    [Fact]
    public async Task El_cultivo_de_otro_usuario_responde_404_sin_pedir_avances()
    {
        var ajeno = await RegistrarAsync(OtroUsuario, "Ajeno");

        var error = await Assert.ThrowsAsync<NoEncontradoException>(() => Detalle().EjecutarAsync(ajeno, Usuario));

        Assert.Equal(ObtenerDetalleCultivoHandler.NoEncontrado, error.Message);
        Assert.Equal(0, _procedimientos.LlamadasAvances);
    }

    [Fact]
    public async Task Un_cultivo_que_no_existe_responde_404()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => Detalle().EjecutarAsync(999, Usuario));
    }

    // ----- Fecha de México -----

    [Theory]
    [InlineData("2026-10-28T21:00:00Z", "2026-10-28")]   // 15:00 en México
    [InlineData("2026-10-29T03:30:00Z", "2026-10-28")]   // 21:30 del día anterior en México
    [InlineData("2026-10-29T06:30:00Z", "2026-10-29")]   // 00:30 en México
    public void FechaMexico_devuelve_el_dia_en_hora_de_Mexico(string utc, string esperado)
    {
        var hoy = FechaMexico.Hoy(new RelojFijo(DateTimeOffset.Parse(utc)));

        Assert.Equal(DateOnly.Parse(esperado), hoy);
    }
}
