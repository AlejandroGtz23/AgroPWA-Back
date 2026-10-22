using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

/// <summary>
/// se protegen las contraseñas, nunca se guardan en texto,
/// se genera un hash el cual se guarda en usuario.contra.hash
/// se comprueba si la contraseña es la correcta.
/// </summary>
public interface IHashContrasena
{
    string Generar(string contrasena);
    bool Verificar(string contrasena, string hash);
}

