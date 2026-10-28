using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Models;
using Agro.Application.Dashboard.Common;
using Agro.Application.Dashboard.Queries.ObtenerResumenDashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Tests.Application.Dashboard;

public class ObtenerResumenDashboardHandlerTests
{
    private const int Usuario = 1;

    private readonly DashboardProcedimientosFalsos _procedimientos = new();
    private ObtenerResumenDashboardHandler Handler() => new(_procedimientos);

    [Fact]
    public async Task Un_usuario_sin_datos_ve_todo_en_cero()
    {
        // Preparar
        _procedimientos.Totales = new TotalesDashboard("Ana", 0, 0, false, false, false);

        // Ejecutar
        var resumen = await Handler().EjecutarAsync(Usuario);

        // Comprobar
        Assert.Equal("Ana", resumen.Nombre);
        Assert.Equal(0, resumen.CultivosActivos);
        Assert.Equal(0, resumen.AvancesDelMes);
        Assert.False(resumen.Guia.TieneParcela);
        Assert.False(resumen.Guia.TieneCultivo);
        Assert.False(resumen.Guia.TieneAvance);
        Assert.Empty(resumen.ActividadReciente);
    }

    [Fact]
    public async Task Arma_el_resumen_con_los_totales_y_la_guia()
    {
        _procedimientos.Totales = new TotalesDashboard("Ana", 2, 3, true, true, false);

        var resumen = await Handler().EjecutarAsync(Usuario);

        Assert.Equal(2, resumen.CultivosActivos);
        Assert.Equal(3, resumen.AvancesDelMes);
        Assert.True(resumen.Guia.TieneParcela);
        Assert.True(resumen.Guia.TieneCultivo);
        Assert.False(resumen.Guia.TieneAvance);
        Assert.Equal(Usuario, _procedimientos.UsuarioConsultado);   // solo datos del usuario del token
    }

    [Fact]
    public async Task Incluye_la_actividad_reciente_y_pide_maximo_5()
    {
        _procedimientos.Totales = new TotalesDashboard("Ana", 1, 2, true, true, true);
        _procedimientos.Actividad.Add(new ActividadRecienteDto("Riego", "Maíz criollo", DateTime.UtcNow));
        _procedimientos.Actividad.Add(new ActividadRecienteDto("Fertilización", "Frijol", DateTime.UtcNow.AddDays(-1)));

        var resumen = await Handler().EjecutarAsync(Usuario);

        Assert.Equal(2, resumen.ActividadReciente.Count);
        Assert.Equal("Riego", resumen.ActividadReciente[0].Tipo);
        Assert.Equal("Maíz criollo", resumen.ActividadReciente[0].Cultivo);
        Assert.Equal(ObtenerResumenDashboardHandler.LimiteActividad, _procedimientos.LimitePedido);
    }

    [Fact]
    public async Task Si_el_usuario_no_existe_lanza_404()
    {
        // Totales queda en null: así responde el procedimiento si el usuario no existe o está desactivado

        await Assert.ThrowsAsync<NoEncontradoException>(() => Handler().EjecutarAsync(Usuario));

        Assert.Null(_procedimientos.LimitePedido);   // se detuvo antes de pedir la actividad
    }

}
