using Agro.Application.Common.Interfaces;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Security;
/// <summary>
/// Hash de contraseñas con BCrypt 
/// </summary>
public sealed class HashContrasenaBCrypt :  IHashContrasena
{
    private const int Costo = 12;
    public string Generar(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena, Costo);
    public bool Verificar(string contrasena, string hash) => BCrypt.Net.BCrypt.Verify(contrasena, hash);

}
