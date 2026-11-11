using Agro.Application.Common.Exceptions;
using Agro.Application.Fotografias.Commands.EliminarFotografia;
using Agro.Application.Fotografias.Commands.SubirFotografia;
using Agro.Application.Fotografias.Common;
using FluentValidation;

namespace Agro.Tests.Application.Fotografias;

/// <summary>Pruebas de subir y eliminar fotografías (HU-20).</summary>
public class FotografiasTests
{
    private const int Usuario = 3;
    private const int OtroUsuario = 4;
    private const int Cultivo = 12;

    // Primeros bytes («firma») de cada formato, más algunos bytes de relleno
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];
    private static readonly byte[] Jpg = [0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3];

    private readonly FotografiasProcedimientosFalsos _procedimientos = new();
    private readonly AlmacenFotosFalso _almacen = new();

    private SubirFotografiaHandler Subir() => new(_procedimientos, _almacen, new SubirFotografiaValidator());
    private EliminarFotografiaHandler Eliminar() => new(_procedimientos, _almacen);

    private static SubirFotografiaCommand Comando(byte[] bytes, long? tamano = null, Guid? idLocal = null,
        bool portada = false) =>
        new(new MemoryStream(bytes), tamano ?? bytes.Length, null, portada, idLocal, null, null);

    // ----- Subir -----

    [Fact]
    public async Task Sube_un_png_con_nombre_generado_y_devuelve_el_enlace()
    {
        var (foto, creada) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png, portada: true));

        var ruta = Assert.Single(_almacen.Subidas);
        Assert.True(creada);
        Assert.StartsWith("usuario-3/cultivo-12/", ruta);
        Assert.EndsWith(".png", foto.NombreArchivo);
        Assert.Equal($"https://almacen.test/{ruta}?sig=prueba", foto.Url);
        Assert.Equal(1, foto.TamanoKb);   // 11 bytes → 1 KB (se redondea hacia arriba)
        Assert.True(foto.EsPortada);
    }

    [Fact]
    public async Task Reconoce_un_jpg_por_su_firma()
    {
        var (foto, _) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Jpg));

        Assert.EndsWith(".jpg", foto.NombreArchivo);
    }

    [Fact]
    public async Task Rechaza_un_archivo_que_no_es_imagen_sin_subirlo()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Subir().EjecutarAsync(Cultivo, Usuario, Comando("no soy una imagen"u8.ToArray())));

        Assert.Contains(error.Errors, e => e.PropertyName == "archivo" && e.ErrorMessage == SubirFotografiaValidator.MensajeFormato);
        Assert.Empty(_almacen.Subidas);
    }

    [Theory]
    [InlineData(0, "Selecciona una imagen.")]
    [InlineData(SubirFotografiaValidator.TamanoMaximo + 1, SubirFotografiaValidator.MensajeFormato)]
    public async Task Rechaza_un_archivo_vacio_o_de_mas_de_5_MB(long tamano, string mensaje)
    {
        var error = await Assert.ThrowsAsync<ValidationException>(
            () => Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png, tamano)));

        Assert.Equal(mensaje, Assert.Single(error.Errors).ErrorMessage);
        Assert.Empty(_almacen.Subidas);
    }

    [Fact]
    public async Task Si_la_base_de_datos_falla_borra_el_archivo_subido()
    {
        _procedimientos.ErrorAlRegistrar = new NoEncontradoException("Cultivo no encontrado.");

        await Assert.ThrowsAsync<NoEncontradoException>(() => Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png)));

        Assert.Equal(_almacen.Subidas, _almacen.Eliminadas);   // no quedan archivos sueltos
    }

    [Fact]
    public async Task Con_el_mismo_idLocal_devuelve_la_existente_y_borra_la_copia()
    {
        var idLocal = Guid.Parse("dddddddd-4444-4444-4444-444444444444");
        var (primera, _) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png, idLocal: idLocal));

        var (segunda, creada) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png, idLocal: idLocal));

        Assert.False(creada);   // el controlador responde 200 en lugar de 201
        Assert.Equal(primera.Id, segunda.Id);
        Assert.Single(_procedimientos.Fotos);
        Assert.Equal(_almacen.Subidas[1], Assert.Single(_almacen.Eliminadas));
    }

    [Fact]
    public void Rechaza_coordenadas_fuera_de_rango()
    {
        var resultado = new SubirFotografiaValidator().Validate(Comando(Png) with { Latitud = 91, Longitud = -181 });

        Assert.Equal(["Latitud", "Longitud"], resultado.Errors.Select(e => e.PropertyName).Order());
    }

    [Fact]
    public async Task Detectar_el_tipo_deja_el_archivo_al_inicio()
    {
        var contenido = new MemoryStream(Png);

        await TipoImagen.DetectarAsync(contenido);

        Assert.Equal(0, contenido.Position);   // para que después se suba completo
    }

    // ----- Eliminar -----

    [Fact]
    public async Task Elimina_la_foto_y_su_archivo()
    {
        var (foto, _) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png));

        await Eliminar().EjecutarAsync(foto.Id, Usuario);

        Assert.Empty(_procedimientos.Fotos);
        Assert.Contains(_almacen.Subidas[0], _almacen.Eliminadas);
    }

    [Fact]
    public async Task Eliminar_una_foto_de_otro_usuario_responde_404_y_no_la_borra()
    {
        var (foto, _) = await Subir().EjecutarAsync(Cultivo, Usuario, Comando(Png));

        var error = await Assert.ThrowsAsync<NoEncontradoException>(() => Eliminar().EjecutarAsync(foto.Id, OtroUsuario));

        Assert.Equal(EliminarFotografiaHandler.NoEncontrada, error.Message);
        Assert.Single(_procedimientos.Fotos);
    }
}