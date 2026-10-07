using System;
using System.Collections.Generic;
using Agro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agro.Infrastructure.Persistence;

public partial class AgroDbContext : DbContext
{
    public AgroDbContext(DbContextOptions<AgroDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Avance> Avance { get; set; }

    public virtual DbSet<CodigoVerificacion> CodigoVerificacion { get; set; }

    public virtual DbSet<ColaSincronizacion> ColaSincronizacion { get; set; }

    public virtual DbSet<Cultivo> Cultivo { get; set; }

    public virtual DbSet<Dispositivo> Dispositivo { get; set; }

    public virtual DbSet<Fotografia> Fotografia { get; set; }

    public virtual DbSet<MensajeContacto> MensajeContacto { get; set; }

    public virtual DbSet<Parcela> Parcela { get; set; }

    public virtual DbSet<Rol> Rol { get; set; }

    public virtual DbSet<Sesion> Sesion { get; set; }

    public virtual DbSet<TipoActividad> TipoActividad { get; set; }

    public virtual DbSet<TipoCultivo> TipoCultivo { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Avance>(entity =>
        {
            entity.HasKey(e => e.IdAvance).HasName("pk_avance");

            entity.ToTable("avance");

            entity.HasIndex(e => new { e.IdCultivo, e.FechaActividad }, "ix_avance_cultivo_fecha").IsDescending(false, true);

            entity.HasIndex(e => e.IdTipoActividad, "ix_avance_tipo");

            entity.HasIndex(e => e.IdUsuario, "ix_avance_usuario");

            entity.HasIndex(e => new { e.IdAvance, e.IdCultivo }, "uq_avance_cultivo").IsUnique();

            entity.HasIndex(e => new { e.IdUsuario, e.IdLocal }, "uq_avance_usuario_local").IsUnique();

            entity.Property(e => e.IdAvance)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_avance");
            entity.Property(e => e.Cantidad)
                .HasPrecision(10, 2)
                .HasColumnName("cantidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.DuracionMin).HasColumnName("duracion_min");
            entity.Property(e => e.EstadoSincronizacion)
                .HasMaxLength(12)
                .HasDefaultValueSql("'sincronizado'::character varying")
                .HasColumnName("estado_sincronizacion");
            entity.Property(e => e.FechaActividad).HasColumnName("fecha_actividad");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdCultivo).HasColumnName("id_cultivo");
            entity.Property(e => e.IdLocal).HasColumnName("id_local");
            entity.Property(e => e.IdTipoActividad).HasColumnName("id_tipo_actividad");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.MetodoRiego)
                .HasMaxLength(30)
                .HasColumnName("metodo_riego");
            entity.Property(e => e.Producto)
                .HasMaxLength(100)
                .HasColumnName("producto");
            entity.Property(e => e.Unidad)
                .HasMaxLength(20)
                .HasColumnName("unidad");

            entity.HasOne(d => d.IdTipoActividadNavigation).WithMany(p => p.Avance)
                .HasForeignKey(d => d.IdTipoActividad)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_avance_tipo");

            entity.HasOne(d => d.Cultivo).WithMany(p => p.Avance)
                .HasPrincipalKey(p => new { p.IdCultivo, p.IdUsuario })
                .HasForeignKey(d => new { d.IdCultivo, d.IdUsuario })
                .HasConstraintName("fk_avance_cultivo");
        });

        modelBuilder.Entity<CodigoVerificacion>(entity =>
        {
            entity.HasKey(e => e.IdCodigo).HasName("pk_codigo_verificacion");

            entity.ToTable("codigo_verificacion");

            entity.HasIndex(e => new { e.IdUsuario, e.Tipo }, "ix_codigo_usuario_tipo");

            entity.Property(e => e.IdCodigo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_codigo");
            entity.Property(e => e.CodigoHash)
                .HasMaxLength(255)
                .HasColumnName("codigo_hash");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(e => e.FechaUso).HasColumnName("fecha_uso");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Intentos).HasColumnName("intentos");
            entity.Property(e => e.Tipo)
                .HasMaxLength(25)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.CodigoVerificacion)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_codigo_usuario");
        });

        modelBuilder.Entity<ColaSincronizacion>(entity =>
        {
            entity.HasKey(e => e.IdRegistro).HasName("pk_cola_sincronizacion");

            entity.ToTable("cola_sincronizacion");

            entity.HasIndex(e => new { e.IdDispositivo, e.Estado }, "ix_cola_dispositivo_estado");

            entity.HasIndex(e => new { e.IdUsuario, e.Estado }, "ix_cola_usuario_estado");

            entity.Property(e => e.IdRegistro)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_registro");
            entity.Property(e => e.DatosJson)
                .HasColumnType("jsonb")
                .HasColumnName("datos_json");
            entity.Property(e => e.Entidad)
                .HasMaxLength(20)
                .HasColumnName("entidad");
            entity.Property(e => e.Estado)
                .HasMaxLength(12)
                .HasDefaultValueSql("'pendiente'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaSincronizacion).HasColumnName("fecha_sincronizacion");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdLocal).HasColumnName("id_local");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Intentos).HasColumnName("intentos");
            entity.Property(e => e.MensajeError)
                .HasMaxLength(300)
                .HasColumnName("mensaje_error");
            entity.Property(e => e.Operacion)
                .HasMaxLength(10)
                .HasColumnName("operacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.ColaSincronizacion)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_cola_usuario");

            entity.HasOne(d => d.Dispositivo).WithMany(p => p.ColaSincronizacion)
                .HasPrincipalKey(p => new { p.IdDispositivo, p.IdUsuario })
                .HasForeignKey(d => new { d.IdDispositivo, d.IdUsuario })
                .HasConstraintName("fk_cola_dispositivo");
        });

        modelBuilder.Entity<Cultivo>(entity =>
        {
            entity.HasKey(e => e.IdCultivo).HasName("pk_cultivo");

            entity.ToTable("cultivo");

            entity.HasIndex(e => e.IdParcela, "ix_cultivo_parcela");

            entity.HasIndex(e => e.IdTipoCultivo, "ix_cultivo_tipo");

            entity.HasIndex(e => new { e.IdUsuario, e.Estado }, "ix_cultivo_usuario_estado");

            entity.HasIndex(e => new { e.IdCultivo, e.IdUsuario }, "uq_cultivo_usuario").IsUnique();

            entity.HasIndex(e => new { e.IdUsuario, e.IdLocal }, "uq_cultivo_usuario_local").IsUnique();

            entity.Property(e => e.IdCultivo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_cultivo");
            entity.Property(e => e.Estado)
                .HasMaxLength(12)
                .HasDefaultValueSql("'activo'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.EstadoSincronizacion)
                .HasMaxLength(12)
                .HasDefaultValueSql("'sincronizado'::character varying")
                .HasColumnName("estado_sincronizacion");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCosecha).HasColumnName("fecha_cosecha");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaSiembra).HasColumnName("fecha_siembra");
            entity.Property(e => e.IdLocal).HasColumnName("id_local");
            entity.Property(e => e.IdParcela).HasColumnName("id_parcela");
            entity.Property(e => e.IdTipoCultivo).HasColumnName("id_tipo_cultivo");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(300)
                .HasColumnName("observaciones");
            entity.Property(e => e.Variedad)
                .HasMaxLength(80)
                .HasColumnName("variedad");

            entity.HasOne(d => d.IdTipoCultivoNavigation).WithMany(p => p.Cultivo)
                .HasForeignKey(d => d.IdTipoCultivo)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_cultivo_tipo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cultivo)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_cultivo_usuario");

            entity.HasOne(d => d.Parcela).WithMany(p => p.Cultivo)
                .HasPrincipalKey(p => new { p.IdParcela, p.IdUsuario })
                .HasForeignKey(d => new { d.IdParcela, d.IdUsuario })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cultivo_parcela");
        });

        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.IdDispositivo).HasName("pk_dispositivo");

            entity.ToTable("dispositivo");

            entity.HasIndex(e => e.IdUsuario, "ix_dispositivo_usuario");

            entity.HasIndex(e => new { e.IdDispositivo, e.IdUsuario }, "uq_dispositivo_id_usuario").IsUnique();

            entity.HasIndex(e => new { e.IdUsuario, e.Identificador }, "uq_dispositivo_usuario").IsUnique();

            entity.Property(e => e.IdDispositivo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_dispositivo");
            entity.Property(e => e.EspacioUsadoMb)
                .HasPrecision(8, 2)
                .HasColumnName("espacio_usado_mb");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Identificador)
                .HasMaxLength(64)
                .HasColumnName("identificador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.SincronizarAuto)
                .HasDefaultValue(true)
                .HasColumnName("sincronizar_auto");
            entity.Property(e => e.SoloWifi).HasColumnName("solo_wifi");
            entity.Property(e => e.UltimaSincronizacion).HasColumnName("ultima_sincronizacion");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Dispositivo)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_dispositivo_usuario");
        });

        modelBuilder.Entity<Fotografia>(entity =>
        {
            entity.HasKey(e => e.IdFotografia).HasName("pk_fotografia");

            entity.ToTable("fotografia");

            entity.HasIndex(e => e.IdAvance, "ix_fotografia_avance");

            entity.HasIndex(e => e.IdCultivo, "ix_fotografia_cultivo");

            entity.HasIndex(e => e.IdUsuario, "ix_fotografia_usuario");

            entity.HasIndex(e => e.IdCultivo, "uq_fotografia_portada")
                .IsUnique()
                .HasFilter("es_portada");

            entity.HasIndex(e => new { e.IdUsuario, e.IdLocal }, "uq_fotografia_usuario_local").IsUnique();

            entity.Property(e => e.IdFotografia)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_fotografia");
            entity.Property(e => e.ArchivoUrl)
                .HasMaxLength(500)
                .HasColumnName("archivo_url");
            entity.Property(e => e.EsPortada).HasColumnName("es_portada");
            entity.Property(e => e.EstadoSincronizacion)
                .HasMaxLength(12)
                .HasDefaultValueSql("'sincronizado'::character varying")
                .HasColumnName("estado_sincronizacion");
            entity.Property(e => e.FechaCaptura)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_captura");
            entity.Property(e => e.IdAvance).HasColumnName("id_avance");
            entity.Property(e => e.IdCultivo).HasColumnName("id_cultivo");
            entity.Property(e => e.IdLocal).HasColumnName("id_local");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Latitud)
                .HasPrecision(10, 8)
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasPrecision(11, 8)
                .HasColumnName("longitud");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(150)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.TamanoKb).HasColumnName("tamano_kb");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Fotografia)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_fotografia_usuario");

            entity.HasOne(d => d.Avance).WithMany(p => p.Fotografia)
                .HasPrincipalKey(p => new { p.IdAvance, p.IdCultivo })
                .HasForeignKey(d => new { d.IdAvance, d.IdCultivo })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_fotografia_avance");

            entity.HasOne(d => d.Cultivo).WithMany(p => p.Fotografia)
                .HasPrincipalKey(p => new { p.IdCultivo, p.IdUsuario })
                .HasForeignKey(d => new { d.IdCultivo, d.IdUsuario })
                .HasConstraintName("fk_fotografia_cultivo");
        });

        modelBuilder.Entity<MensajeContacto>(entity =>
        {
            entity.HasKey(e => e.IdMensaje).HasName("pk_mensaje_contacto");

            entity.ToTable("mensaje_contacto");

            entity.Property(e => e.IdMensaje)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_mensaje");
            entity.Property(e => e.Asunto)
                .HasMaxLength(150)
                .HasColumnName("asunto");
            entity.Property(e => e.Atendido).HasColumnName("atendido");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.FechaEnvio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(1000)
                .HasColumnName("mensaje");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Parcela>(entity =>
        {
            entity.HasKey(e => e.IdParcela).HasName("pk_parcela");

            entity.ToTable("parcela");

            entity.HasIndex(e => new { e.IdUsuario, e.Nombre }, "uq_parcela_nombre_usuario").IsUnique();

            entity.HasIndex(e => new { e.IdParcela, e.IdUsuario }, "uq_parcela_usuario").IsUnique();

            entity.HasIndex(e => new { e.IdUsuario, e.IdLocal }, "uq_parcela_usuario_local").IsUnique();

            entity.Property(e => e.IdParcela)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_parcela");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdLocal).HasColumnName("id_local");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Latitud)
                .HasPrecision(10, 8)
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasPrecision(11, 8)
                .HasColumnName("longitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.SuperficieHa)
                .HasPrecision(8, 2)
                .HasColumnName("superficie_ha");
            entity.Property(e => e.UbicacionReferencia)
                .HasMaxLength(150)
                .HasColumnName("ubicacion_referencia");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Parcela)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_parcela_usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("pk_rol");

            entity.ToTable("rol");

            entity.HasIndex(e => e.Nombre, "uq_rol_nombre").IsUnique();

            entity.Property(e => e.IdRol)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Sesion>(entity =>
        {
            entity.HasKey(e => e.IdSesion).HasName("pk_sesion");

            entity.ToTable("sesion");

            entity.HasIndex(e => e.IdUsuario, "ix_sesion_abiertas").HasFilter("(fecha_cierre IS NULL)");

            entity.HasIndex(e => e.Jti, "uq_sesion_jti").IsUnique();

            entity.HasIndex(e => e.RefreshTokenHash, "uq_sesion_refresh").IsUnique();

            entity.Property(e => e.IdSesion)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_sesion");
            entity.Property(e => e.AgenteUsuario)
                .HasMaxLength(300)
                .HasColumnName("agente_usuario");
            entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
            entity.Property(e => e.FechaExpiracion).HasColumnName("fecha_expiracion");
            entity.Property(e => e.FechaInicio)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_inicio");
            entity.Property(e => e.IdDispositivo).HasColumnName("id_dispositivo");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Ip)
                .HasMaxLength(45)
                .HasColumnName("ip");
            entity.Property(e => e.Jti)
                .HasMaxLength(64)
                .HasColumnName("jti");
            entity.Property(e => e.MotivoCierre)
                .HasMaxLength(20)
                .HasColumnName("motivo_cierre");
            entity.Property(e => e.RefreshTokenHash)
                .HasMaxLength(255)
                .HasColumnName("refresh_token_hash");
            entity.Property(e => e.UltimaActividad)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("ultima_actividad");

            entity.HasOne(d => d.IdDispositivoNavigation).WithMany(p => p.Sesion)
                .HasForeignKey(d => d.IdDispositivo)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_sesion_dispositivo");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Sesion)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("fk_sesion_usuario");
        });

        modelBuilder.Entity<TipoActividad>(entity =>
        {
            entity.HasKey(e => e.IdTipoActividad).HasName("pk_tipo_actividad");

            entity.ToTable("tipo_actividad");

            entity.HasIndex(e => e.Nombre, "uq_tipo_actividad_nombre").IsUnique();

            entity.Property(e => e.IdTipoActividad)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_actividad");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoCultivo>(entity =>
        {
            entity.HasKey(e => e.IdTipoCultivo).HasName("pk_tipo_cultivo");

            entity.ToTable("tipo_cultivo");

            entity.HasIndex(e => e.Nombre, "uq_tipo_cultivo_nombre").IsUnique();

            entity.Property(e => e.IdTipoCultivo)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_tipo_cultivo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("pk_usuario");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.IdRol, "ix_usuario_rol");

            entity.Property(e => e.IdUsuario)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(60)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.BloqueadoHasta).HasColumnName("bloqueado_hasta");
            entity.Property(e => e.ComunidadMunicipio)
                .HasMaxLength(120)
                .HasColumnName("comunidad_municipio");
            entity.Property(e => e.ContrasenaHash)
                .HasMaxLength(255)
                .HasColumnName("contrasena_hash");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.CorreoVerificado).HasColumnName("correo_verificado");
            entity.Property(e => e.DosPasosActivo)
                .HasDefaultValue(true)
                .HasColumnName("dos_pasos_activo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IntentosFallidos).HasColumnName("intentos_fallidos");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .HasColumnName("telefono");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuario)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_usuario_rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
