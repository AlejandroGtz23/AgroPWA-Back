using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Common;

public sealed record EstadoSincronizacionDto(int IdDispositivo, string Nombre, bool SincronizarAuto, bool SoloWifi,
    decimal EspacioUsadoMb, DateTimeOffset? UltimaSincronizacion, int Errores);