using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;
/// <summary>
/// Se crean los tokens de la sesion
/// </summary>
public interface IGeneradorJwt
{
    TokenGenerado GenerarAccessToken(int idUsuario, string correo, string rol, string jti);
    (string Token, string Hash) GenerarRefreshToken();
    string HashRefreshToken(string refreshToken);

}
    

