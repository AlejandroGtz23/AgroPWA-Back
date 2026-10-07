using Agro.Application.Common.Interfaces;
using Agro.Application.Salud.Queries.ComprobarSalud;
using Agro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agro.Tests.Application.Salud;

/// <summary>
/// Pruebas del query ComprobarSalud (HU-04, criterio 3).
/// No usan base de datos real: se reemplaza IAgroDbContext por un objeto falso.
/// </summary>
public class ComprobarSaludHandlerTests
{
    [Fact]
    public async Task Responde_OK_cuando_la_base_de_datos_conecta()
    {
        var handler = new ComprobarSaludHandler(new AgroDbContextFalso(conecta: true));

        var salud = await handler.EjecutarAsync();

        Assert.Equal("OK", salud.Estado);
        Assert.Equal("conectada", salud.BaseDatos);
        Assert.Null(salud.Mensaje);
    }

    [Fact]
    public async Task Responde_ERROR_cuando_la_base_de_datos_no_conecta()
    {
        var handler = new ComprobarSaludHandler(new AgroDbContextFalso(conecta: false));

        var salud = await handler.EjecutarAsync();

        Assert.Equal("ERROR", salud.Estado);
        Assert.Equal("sin conexión", salud.BaseDatos);
        Assert.NotNull(salud.Mensaje);
    }

    [Fact]
    public async Task Responde_ERROR_sin_lanzar_excepcion_si_la_conexion_falla()
    {
        var handler = new ComprobarSaludHandler(new AgroDbContextFalso(lanzaError: true));

        var salud = await handler.EjecutarAsync();

        Assert.Equal("ERROR", salud.Estado);
    }

    [Fact]
    public async Task La_fecha_se_devuelve_en_UTC()
    {
        var handler = new ComprobarSaludHandler(new AgroDbContextFalso(conecta: true));

        var salud = await handler.EjecutarAsync();

        Assert.Equal(DateTimeKind.Utc, salud.Fecha.Kind);
    }

    /// <summary>Contexto falso: solo simula si la base de datos responde.</summary>
    private sealed class AgroDbContextFalso(bool conecta = false, bool lanzaError = false) : IAgroDbContext
    {
        public Task<bool> PuedeConectarAsync(CancellationToken cancellationToken = default)
            => lanzaError ? throw new TimeoutException("Sin respuesta") : Task.FromResult(conecta);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

        // Estas pruebas no consultan tablas
        public DbSet<Rol> Rol => throw new NotSupportedException();
        public DbSet<TipoCultivo> TipoCultivo => throw new NotSupportedException();
        public DbSet<TipoActividad> TipoActividad => throw new NotSupportedException();
        public DbSet<Usuario> Usuario => throw new NotSupportedException();
        public DbSet<Dispositivo> Dispositivo => throw new NotSupportedException();
        public DbSet<Sesion> Sesion => throw new NotSupportedException();
        public DbSet<CodigoVerificacion> CodigoVerificacion => throw new NotSupportedException();
        public DbSet<Parcela> Parcela => throw new NotSupportedException();
        public DbSet<Cultivo> Cultivo => throw new NotSupportedException();
        public DbSet<Avance> Avance => throw new NotSupportedException();
        public DbSet<Fotografia> Fotografia => throw new NotSupportedException();
        public DbSet<ColaSincronizacion> ColaSincronizacion => throw new NotSupportedException();
        public DbSet<MensajeContacto> MensajeContacto => throw new NotSupportedException();
    }
}
