using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common;

public static class FechaMexico
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    public static DateOnly Hoy(TimeProvider reloj) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), Zona).DateTime);
}
