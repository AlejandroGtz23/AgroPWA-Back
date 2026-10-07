using Agro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agro.Application.Common.Interfaces;

/// <summary>
/// Contrato de acceso a datos para los commands y queries.
/// Application lo usa sin saber que detrás hay PostgreSQL; Infrastructure lo implementa.
/// </summary>
public interface IAgroDbContext
{
    // Catálogos
    DbSet<Rol> Rol { get; }
    DbSet<TipoCultivo> TipoCultivo { get; }
    DbSet<TipoActividad> TipoActividad { get; }

    // Usuarios y seguridad
    DbSet<Usuario> Usuario { get; }
    DbSet<Dispositivo> Dispositivo { get; }
    DbSet<Sesion> Sesion { get; }
    DbSet<CodigoVerificacion> CodigoVerificacion { get; }

    // Cultivos
    DbSet<Parcela> Parcela { get; }
    DbSet<Cultivo> Cultivo { get; }
    DbSet<Avance> Avance { get; }
    DbSet<Fotografia> Fotografia { get; }

    // Sincronización y contacto
    DbSet<ColaSincronizacion> ColaSincronizacion { get; }
    DbSet<MensajeContacto> MensajeContacto { get; }

    /// <summary>Guarda los cambios pendientes en la base de datos.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Comprueba si la base de datos responde (lo usa /api/salud).</summary>
    Task<bool> PuedeConectarAsync(CancellationToken cancellationToken = default);
}
