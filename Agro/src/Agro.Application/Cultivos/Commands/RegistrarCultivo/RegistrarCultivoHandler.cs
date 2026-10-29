using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using FluentValidation;

namespace Agro.Application.Cultivos.Commands.RegistrarCultivo;

public sealed class RegistrarCultivoHandler(
    ICultivosProcedimientos procedimientos,
    IValidator<RegistrarCultivoCommand> validador)
{
    public async Task<(CultivoDto Cultivo, bool Creado)> EjecutarAsync(int idUsuario,
        RegistrarCultivoCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                              // 400

        var datos = new NuevoCultivo(
            comando.IdParcela!.Value, comando.IdTipoCultivo!.Value, comando.Nombre.Trim(),
            string.IsNullOrWhiteSpace(comando.Variedad) ? null : comando.Variedad.Trim(),
            comando.FechaSiembra!.Value,
            string.IsNullOrWhiteSpace(comando.Observaciones) ? null : comando.Observaciones.Trim());

        // 400 si la parcela no es del usuario o el tipo no existe (lo indica el procedimiento)
        var registro = await procedimientos.RegistrarAsync(idUsuario, comando.IdLocal, datos, ct);

        var detalle = await procedimientos.ObtenerDetalleAsync(registro.IdCultivo, idUsuario, ct);
        return (detalle!.Cultivo, !registro.YaExistia);
    }
}