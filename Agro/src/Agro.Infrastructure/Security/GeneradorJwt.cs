using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Infrastructure.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Agro.Infrastructure.Security;
public sealed class GeneradorJwt(ConfiguracionJwt configuracion, TimeProvider reloj) : IGeneradorJwt
{
    public TokenGenerado GenerarAccessToken(int idUsuario, string correo, string rol, string jti)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddMinutes(configuracion.MinutosAcceso);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = configuracion.Emisor,
            Audience = configuracion.Audiencia,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = idUsuario.ToString(),
                [JwtRegisteredClaimNames.Email] = correo,
                [JwtRegisteredClaimNames.Jti] = jti,
                ["role"] = rol
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracion.Clave)),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenGenerado(new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }

    public (string Token, string Hash) GenerarRefreshToken()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        return (token, HashRefreshToken(token));
    }
    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

}
