using Agro.Application.Common.Exceptions;
using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Parcelas.Queries.ObtenerMisParcelas;
using FluentValidation;

namespace Agro.Tests.Application.Parcelas;

/// <summary>Pruebas de registrar, editar y consultar parcelas (HU-13).</summary>
public class ParcelasHandlersTests
{
    private const int Usuario = 1;
    private const int OtroUsuario = 2;
    private static readonly Guid IdLocal = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private readonly ParcelasProcedimientosFalsos _procedimientos = new();

    private RegistrarParcelaHandler Registrar() => new(_procedimientos, new RegistrarParcelaValidator());
    private EditarParcelaHandler Editar() => new(_procedimientos, new EditarParcelaValidator());

    private static RegistrarParcelaCommand Nueva(string nombre = "Parcela Norte", Guid? idLocal = null,
        decimal superficie = 1.5m, string? ubicacion = "San Marcos") =>
        new(idLocal, nombre, superficie, ubicacion);

    // ----- Registrar -----

    [Fact]
    public async Task Registrar_crea_la_parcela_y_la_devuelve()
    {
        var (parcela, creada) = await Registrar().EjecutarAsync(Usuario, Nueva(idLocal: IdLocal));

        Assert.True(creada);
        Assert.Equal("Parcela Norte", parcela.Nombre);
        Assert.Equal(1.5m, parcela.SuperficieHa);
        Assert.Equal(IdLocal, parcela.IdLocal);
        Assert.Equal(0, parcela.NumeroCultivos);
    }

    [Fact]
    public async Task Registrar_con_el_mismo_idLocal_devuelve_la_existente_sin_duplicar()
    {
        var (primera, _) = await Registrar().EjecutarAsync(Usuario, Nueva(idLocal: IdLocal));

        var (segunda, creada) = await Registrar().EjecutarAsync(Usuario, Nueva(idLocal: IdLocal));

        Assert.False(creada);   // el controlador responde 200 en lugar de 201
        Assert.Equal(primera.Id, segunda.Id);
        Assert.Single(await _procedimientos.ObtenerAsync(Usuario));
    }

    [Fact]
    public async Task Registrar_quita_espacios_y_guarda_la_ubicacion_vacia_como_null()
    {
        var (parcela, _) = await Registrar().EjecutarAsync(Usuario, Nueva(nombre: "  Parcela Sur  ", ubicacion: "   "));

        Assert.Equal("Parcela Sur", parcela.Nombre);
        Assert.Null(parcela.UbicacionReferencia);
    }

    [Fact]
    public async Task Registrar_un_nombre_repetido_lanza_409()
    {
        await Registrar().EjecutarAsync(Usuario, Nueva());

        await Assert.ThrowsAsync<ConflictoException>(
            () => Registrar().EjecutarAsync(Usuario, Nueva(nombre: "PARCELA NORTE")));
    }

    [Fact]
    public async Task Otro_usuario_puede_usar_el_mismo_nombre()
    {
        await Registrar().EjecutarAsync(Usuario, Nueva());

        var (_, creada) = await Registrar().EjecutarAsync(OtroUsuario, Nueva());

        Assert.True(creada);
    }

    [Fact]
    public async Task Registrar_datos_invalidos_lanza_400_sin_llamar_al_procedimiento()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Registrar().EjecutarAsync(Usuario, Nueva(nombre: "", superficie: 0)));

        Assert.Equal(0, _procedimientos.LlamadasRegistrar);
    }

    // ----- Editar -----

    [Fact]
    public async Task Editar_actualiza_y_devuelve_la_parcela()
    {
        var (parcela, _) = await Registrar().EjecutarAsync(Usuario, Nueva());

        var editada = await Editar().EjecutarAsync(parcela.Id, Usuario,
            new EditarParcelaCommand(" Parcela Norte Grande ", 2.25m, "Ejido Centro"));

        Assert.Equal(parcela.Id, editada.Id);
        Assert.Equal("Parcela Norte Grande", editada.Nombre);
        Assert.Equal(2.25m, editada.SuperficieHa);
        Assert.Equal("Ejido Centro", editada.UbicacionReferencia);
    }

    [Fact]
    public async Task Editar_una_parcela_que_no_existe_lanza_404()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(() => Editar().EjecutarAsync(
            99, Usuario, new EditarParcelaCommand("Parcela", 1m, null)));
    }

    [Fact]
    public async Task Editar_la_parcela_de_otro_usuario_lanza_404()
    {
        var (ajena, _) = await Registrar().EjecutarAsync(OtroUsuario, Nueva());

        await Assert.ThrowsAsync<NoEncontradoException>(() => Editar().EjecutarAsync(
            ajena.Id, Usuario, new EditarParcelaCommand("Robada", 1m, null)));
    }

    [Fact]
    public async Task Editar_a_un_nombre_que_ya_tiene_lanza_409()
    {
        var (norte, _) = await Registrar().EjecutarAsync(Usuario, Nueva());
        await Registrar().EjecutarAsync(Usuario, Nueva(nombre: "Parcela Sur"));

        await Assert.ThrowsAsync<ConflictoException>(() => Editar().EjecutarAsync(
            norte.Id, Usuario, new EditarParcelaCommand("parcela sur", 1m, null)));
    }

    [Fact]
    public async Task Editar_con_datos_invalidos_lanza_400_sin_editar()
    {
        var (parcela, _) = await Registrar().EjecutarAsync(Usuario, Nueva());

        await Assert.ThrowsAsync<ValidationException>(() => Editar().EjecutarAsync(
            parcela.Id, Usuario, new EditarParcelaCommand("", -1m, null)));

        Assert.Equal("Parcela Norte", (await _procedimientos.ObtenerAsync(Usuario))[0].Nombre);
    }

    // ----- Consultar -----

    [Fact]
    public async Task Mis_parcelas_solo_devuelve_las_del_usuario()
    {
        await Registrar().EjecutarAsync(Usuario, Nueva(nombre: "Parcela Sur"));
        await Registrar().EjecutarAsync(Usuario, Nueva(nombre: "Parcela Norte"));
        await Registrar().EjecutarAsync(OtroUsuario, Nueva(nombre: "Ajena"));

        var parcelas = await new ObtenerMisParcelasHandler(_procedimientos).EjecutarAsync(Usuario);

        Assert.Equal(["Parcela Norte", "Parcela Sur"], parcelas.Select(p => p.Nombre));
        Assert.Equal(Usuario, _procedimientos.UsuarioConsultado);
    }

    [Fact]
    public async Task Sin_parcelas_devuelve_una_lista_vacia()
    {
        var parcelas = await new ObtenerMisParcelasHandler(_procedimientos).EjecutarAsync(Usuario);

        Assert.Empty(parcelas);
    }
}
