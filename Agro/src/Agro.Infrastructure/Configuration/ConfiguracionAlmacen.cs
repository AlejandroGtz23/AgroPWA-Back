using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Infrastructure.Configuration;

public sealed record ConfiguracionAlmacen(string Conexion, string Contenedor)
{
    public static ConfiguracionAlmacen DesdeVariables() => new(
        VariablesEntorno.Obtener("STORAGE_CONEXION"),
        Environment.GetEnvironmentVariable("STORAGE_CONTENEDOR") is { Length: > 0 } contenedor ? contenedor : "fotografias");
}
