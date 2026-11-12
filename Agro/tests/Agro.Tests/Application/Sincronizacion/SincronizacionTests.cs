using Agro.Application.Avances.Commands.RegistrarAvance;
using Agro.Application.Common.Exceptions;
using Agro.Application.Cultivos.Commands.EditarCultivo;
using Agro.Application.Cultivos.Commands.EliminarCultivo;
using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using Agro.Application.Fotografias.Commands.EliminarFotografia;
using Agro.Application.Fotografias.Common;
using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;
using Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;
using Agro.Application.Sincronizacion.Commands.RegistrarDispositivo;
using Agro.Application.Sincronizacion.Common;
using Agro.Application.Sincronizacion.Queries.ObtenerEstado;
using Agro.Tests.Application.Avances;
using Agro.Tests.Application.Cultivos;
using Agro.Tests.Application.Fotografias;
using Agro.Tests.Application.Parcelas;
using FluentValidation;
using System.Text.Json;

namespace Agro.Tests.Application.Sincronizacion;

/// <summary>Pruebas de dispositivos y del envío por lotes (HU-22, criterios 1 a 3).</summary>
public class SincronizacionTests
{
    private const int Usuario = 1;
    private const int Dispositivo = SincronizacionProcedimientosFalsos.MiDispositivo;
    private static readonly RelojFijo Reloj = new(new DateTimeOffset(2026, 11, 11, 18, 0, 0, TimeSpan.Zero));
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly Guid Parcela = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Cultivo = Guid.Parse("11111111-0000-0000-0000-000000000002");
    private static readonly Guid Avance = Guid.Parse("11111111-0000-0000-0000-000000000003");
    private static readonly Guid OtroAvance = Guid.Parse("11111111-0000-0000-0000-000000000004");

    private readonly SincronizacionProcedimientosFalsos _sincronizacion = new();
    private readonly ParcelasProcedimientosFalsos _parcelas = new();
    private readonly CultivosProcedimientosFalsos _cultivos = new();
    private readonly AvancesProcedimientosFalsos _avances = new();

    public SincronizacionTests()
    {
        // La parcela y el cultivo que se crean en el lote reciben el id 1 en los falsos
        _cultivos.AgregarParcela(1, Usuario);
        _avances.AgregarCultivo(1, Usuario);
    }

    private ProcesarSincronizacionHandler Handler()
    {
        var almacen = new AlmacenFotosFalso();
        var enlaces = new EnlacesFotografias(almacen);
        var ejecutor = new EjecutorOperaciones(_sincronizacion,
            new RegistrarParcelaHandler(_parcelas, new RegistrarParcelaValidator()),
            new EditarParcelaHandler(_parcelas, new EditarParcelaValidator()),
            new RegistrarCultivoHandler(_cultivos, new RegistrarCultivoValidator(Reloj), enlaces),
            new EditarCultivoHandler(_cultivos, new EditarCultivoValidator(Reloj), enlaces),
            new EliminarCultivoHandler(_cultivos, almacen),
            new RegistrarAvanceHandler(_avances, new RegistrarAvanceValidator(Reloj)),
            new EliminarFotografiaHandler(new FotografiasProcedimientosFalsos(), almacen));
        return new ProcesarSincronizacionHandler(_sincronizacion, ejecutor, new ProcesarSincronizacionValidator());
    }

    private static OperacionSincronizacion Op(string entidad, Guid idLocal, string operacion, object datos, int minuto) =>
        new(entidad, idLocal, operacion, JsonSerializer.SerializeToElement(datos, Json),
            new DateTimeOffset(2026, 11, 11, 10, minuto, 0, TimeSpan.Zero));

    private static OperacionSincronizacion CrearParcela(int minuto = 0) =>
        Op("parcela", Parcela, "crear", new { nombre = "Parcela sin señal", superficieHa = 2.5m }, minuto);

    private static OperacionSincronizacion CrearCultivo(int minuto = 1) =>
        Op("cultivo", Cultivo, "crear",
            new { nombre = "Frijol", idTipoCultivo = 2, fechaSiembra = "2026-11-01", idLocalParcela = Parcela }, minuto);

    private static OperacionSincronizacion CrearAvance(Guid idLocal, int minuto, string descripcion = "Ya brotó") =>
        Op("avance", idLocal, "crear",
            new { idLocalCultivo = Cultivo, idTipoActividad = 3, fechaActividad = "2026-11-10T15:00:00Z", descripcion }, minuto);

    private static ProcesarSincronizacionCommand Lote(params OperacionSincronizacion[] operaciones) => new(Dispositivo, operaciones);

    // ----- Lote -----

    [Fact]
    public async Task Procesa_en_orden_de_creacion_y_traduce_los_idLocal()
    {
        // Llegan desordenadas: el avance necesita al cultivo y el cultivo a la parcela
        var resultado = await Handler().EjecutarAsync(Usuario, Lote(CrearAvance(Avance, 2), CrearParcela(), CrearCultivo()));

        Assert.Equal(3, resultado.Sincronizados);
        Assert.Equal(0, resultado.Errores);
        Assert.Equal(["parcela", "cultivo", "avance"], resultado.Resultados.Select(r => r.Entidad));
        Assert.All(resultado.Resultados, r => Assert.Equal(1, r.Id));
    }

    [Fact]
    public async Task Una_operacion_que_falla_no_detiene_las_demas()
    {
        var resultado = await Handler().EjecutarAsync(Usuario,
            Lote(CrearParcela(), CrearCultivo(), CrearAvance(Avance, 2, descripcion: ""), CrearAvance(OtroAvance, 3)));

        Assert.Equal(3, resultado.Sincronizados);
        var error = Assert.Single(resultado.Resultados, r => r.Estado == ProcesarSincronizacionHandler.Error);
        Assert.Equal(Avance, error.IdLocal);
        Assert.Equal("La descripción es obligatoria.", error.MensajeError);
        Assert.Equal(4, _sincronizacion.Cola.Count);   // todas quedan en la cola, con su resultado
        Assert.True(_sincronizacion.Marcado);
    }

    [Fact]
    public async Task Reenviar_el_mismo_lote_devuelve_los_mismos_ids_sin_duplicar()
    {
        var lote = Lote(CrearParcela(), CrearCultivo(), CrearAvance(Avance, 2));

        var primero = await Handler().EjecutarAsync(Usuario, lote);
        var segundo = await Handler().EjecutarAsync(Usuario, lote);

        Assert.Equal(primero.Resultados.Select(r => r.Id), segundo.Resultados.Select(r => r.Id));
        Assert.Equal(1, _parcelas.LlamadasRegistrar - 1);   // la segunda vez la parcela ya existía
    }

    [Fact]
    public async Task Usa_el_id_de_la_base_si_el_registro_se_sincronizo_en_un_envio_anterior()
    {
        _sincronizacion.IdsEnBase[("cultivo", Cultivo)] = 1;

        var resultado = await Handler().EjecutarAsync(Usuario, Lote(CrearAvance(Avance, 0)));

        Assert.Equal(ProcesarSincronizacionHandler.Sincronizado, Assert.Single(resultado.Resultados).Estado);
    }

    [Fact]
    public async Task Una_referencia_que_no_se_ha_sincronizado_da_error()
    {
        var resultado = await Handler().EjecutarAsync(Usuario, Lote(CrearAvance(Avance, 0)));

        Assert.Contains("todavía no está sincronizado", Assert.Single(resultado.Resultados).MensajeError);
    }

    [Fact]
    public async Task Editar_un_cultivo_creado_sin_conexion_aplica_el_cambio()
    {
        var editar = Op("cultivo", Cultivo, "editar", new
        {
            nombre = "Frijol editado",
            idTipoCultivo = 2,
            fechaSiembra = "2026-11-01",
            idLocalParcela = Parcela,
            estado = "activo"
        }, 5);

        var resultado = await Handler().EjecutarAsync(Usuario, Lote(CrearParcela(), CrearCultivo(), editar));

        Assert.Equal(3, resultado.Sincronizados);
        Assert.Equal("Frijol editado", _cultivos.UltimoCambio!.Datos.Nombre);
        Assert.Null(_cultivos.UltimoCambio.FechaModificacion);   // no se descarta por la regla de conflictos
    }

    [Fact]
    public async Task Eliminar_un_registro_que_no_existe_da_error()
    {
        var resultado = await Handler().EjecutarAsync(Usuario,
            Lote(Op("cultivo", Guid.NewGuid(), "eliminar", new { }, 0)));

        Assert.Equal(EjecutorOperaciones.RegistroNoEncontrado, Assert.Single(resultado.Resultados).MensajeError);
    }

    [Fact]
    public async Task Datos_que_no_son_un_objeto_dan_error()
    {
        var operacion = new OperacionSincronizacion("parcela", Parcela, "crear",
            JsonSerializer.SerializeToElement("texto"), null);

        var resultado = await Handler().EjecutarAsync(Usuario, Lote(operacion));

        Assert.Equal(EjecutorOperaciones.DatosInvalidos, Assert.Single(resultado.Resultados).MensajeError);
    }

    [Fact]
    public async Task Con_un_dispositivo_ajeno_responde_404_sin_procesar_nada()
    {
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => Handler().EjecutarAsync(Usuario, new ProcesarSincronizacionCommand(99, [CrearParcela()])));

        Assert.Empty(_sincronizacion.Cola);
    }

    // ----- Validador del lote -----

    [Fact]
    public void Un_lote_vacio_no_es_valido()
    {
        var error = Assert.Single(new ProcesarSincronizacionValidator()
            .Validate(new ProcesarSincronizacionCommand(Dispositivo, [])).Errors);
        Assert.Equal("No hay operaciones para sincronizar.", error.ErrorMessage);
    }

    [Fact]
    public void Rechaza_operaciones_no_permitidas_y_sin_idLocal()
    {
        var resultado = new ProcesarSincronizacionValidator().Validate(Lote(
            Op("fotografia", Parcela, "crear", new { }, 0),
            new OperacionSincronizacion("parcela", null, "crear", JsonSerializer.SerializeToElement(new { }), null)));

        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "La operación no se puede sincronizar.");
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Cada operación necesita su idLocal.");
    }

    // ----- Dispositivos -----

    [Fact]
    public async Task Registra_el_dispositivo_y_devuelve_su_estado()
    {
        var handler = new RegistrarDispositivoHandler(_sincronizacion, new RegistrarDispositivoValidator());

        var (dispositivo, creado) = await handler.EjecutarAsync(Usuario,
            new RegistrarDispositivoCommand("  pc-1  ", "Chrome en Windows"));

        Assert.True(creado);
        Assert.Equal(Dispositivo, dispositivo.IdDispositivo);
    }

    [Fact]
    public async Task Registrar_un_dispositivo_sin_identificador_responde_400()
    {
        var handler = new RegistrarDispositivoHandler(_sincronizacion, new RegistrarDispositivoValidator());

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.EjecutarAsync(Usuario, new RegistrarDispositivoCommand("", "Chrome")));
    }

    [Fact]
    public async Task Actualiza_las_preferencias_del_dispositivo()
    {
        var handler = new ActualizarPreferenciasHandler(_sincronizacion, new ActualizarPreferenciasValidator());

        var estado = await handler.EjecutarAsync(Dispositivo, Usuario, new ActualizarPreferenciasCommand(false, true, 12.5m));

        Assert.Equal(Dispositivo, estado.IdDispositivo);
    }

    [Fact]
    public async Task Un_dispositivo_ajeno_responde_404_en_preferencias_y_en_estado()
    {
        var preferencias = new ActualizarPreferenciasHandler(_sincronizacion, new ActualizarPreferenciasValidator());

        await Assert.ThrowsAsync<NoEncontradoException>(
            () => preferencias.EjecutarAsync(99, Usuario, new ActualizarPreferenciasCommand(true, false, null)));
        await Assert.ThrowsAsync<NoEncontradoException>(
            () => new ObtenerEstadoHandler(_sincronizacion).EjecutarAsync(99, Usuario));
    }

    [Fact]
    public void El_espacio_usado_no_puede_ser_negativo()
    {
        var error = Assert.Single(new ActualizarPreferenciasValidator()
            .Validate(new ActualizarPreferenciasCommand(true, false, -1)).Errors);
        Assert.Equal("El espacio usado no puede ser negativo.", error.ErrorMessage);
    }
}