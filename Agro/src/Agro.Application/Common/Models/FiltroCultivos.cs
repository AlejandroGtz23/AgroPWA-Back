using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;
public sealed record FiltroCultivos(string? Buscar, string? Estado, int? IdTipoCultivo, int Pagina, int Tamano);