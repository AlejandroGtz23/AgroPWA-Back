using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;
/// <summary>
/// Datos para iniciar sesion
/// </summary>
public sealed record UsuarioLogin(int IdUsuario, string Nombre, string Correo, string ContrasenaHash, string Rol, DateTime? BloqueadoHasta);
