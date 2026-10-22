using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Security;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Agro.Tests.Infrastructure.Security;

/// <summary>Pruebas de BCrypt y de la generación de tokens (HU-09, HU-10).</summary>
public class SeguridadTests
{
    // Clave solo para pruebas (no es la de ningún entorno real)
    private static readonly ConfiguracionJwt Configuracion =
        new("clave-de-pruebas-unitarias-con-mas-de-32-caracteres", "agro-api", "agro-pwa", 20);

    private readonly GeneradorJwt _generador = new(Configuracion, TimeProvider.System);

    // ----- BCrypt -----

    [Fact]
    public void BCrypt_genera_un_hash_que_no_es_la_contrasena_y_la_verifica()
    {
        var hash = new HashContrasenaBCrypt();

        var resultado = hash.Generar("Secreta123");

        Assert.StartsWith("$2", resultado);
        Assert.NotEqual("Secreta123", resultado);
        Assert.True(hash.Verificar("Secreta123", resultado));
        Assert.False(hash.Verificar("Incorrecta", resultado));
    }

    // ----- Access token -----

    [Fact]
    public void El_access_token_lleva_los_datos_del_usuario_y_vence_en_20_minutos()
    {
        var antes = DateTime.UtcNow;

        var acceso = _generador.GenerarAccessToken(5, "victor@correo.com", "productor", "jti-123");

        var token = new JsonWebToken(acceso.Token);
        Assert.Equal("5", token.Subject);
        Assert.Equal("jti-123", token.Id);
        Assert.Equal("victor@correo.com", token.GetClaim(JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("productor", token.GetClaim("role").Value);
        Assert.Equal("agro-api", token.Issuer);
        Assert.Contains("agro-pwa", token.Audiences);
        Assert.Equal("HS256", token.Alg);
        Assert.InRange(acceso.ExpiraUtc, antes.AddMinutes(19), antes.AddMinutes(21));
    }

    // ----- Refresh token -----

    [Fact]
    public void El_refresh_token_es_aleatorio_y_su_hash_coincide()
    {
        var (token1, hash1) = _generador.GenerarRefreshToken();
        var (token2, _) = _generador.GenerarRefreshToken();

        Assert.NotEqual(token1, token2);
        Assert.NotEqual(token1, hash1);
        Assert.Equal(hash1, _generador.HashRefreshToken(token1));
        Assert.Equal(64, hash1.Length);   // SHA-256 en hexadecimal
    }
}
