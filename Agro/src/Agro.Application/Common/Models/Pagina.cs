using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record Pagina<T>(IReadOnlyList<T> Elementos, int Total);