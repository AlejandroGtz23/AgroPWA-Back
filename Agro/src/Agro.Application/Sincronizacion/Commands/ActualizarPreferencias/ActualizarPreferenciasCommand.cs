using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;

public sealed record ActualizarPreferenciasCommand(bool SincronizarAuto, bool SoloWifi, decimal? EspacioUsadoMb);