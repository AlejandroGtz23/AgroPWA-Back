using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Queries.ObtenerMisCultivos;

public sealed record ObtenerMisCultivosQuery(string? Buscar, string? Estado, int? IdTipoCultivo, int Pagina=1, int Tamano = 20);
