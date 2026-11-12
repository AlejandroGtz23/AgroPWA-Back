using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record DispositivoRegistrado(int IdDispositivo, bool YaExistia);