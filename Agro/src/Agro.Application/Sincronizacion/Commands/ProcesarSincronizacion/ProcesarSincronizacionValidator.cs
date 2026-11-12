using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;


public sealed class ProcesarSincronizacionValidator : AbstractValidator<ProcesarSincronizacionCommand>
{
    public const int MaximoOperaciones = 100;

    /// <summary>Lo que se puede sincronizar (las fotos se suben con su propio endpoint, HU-20).</summary>
    public static readonly string[] Permitidas =
        ["parcela:crear", "parcela:editar", "cultivo:crear", "cultivo:editar", "cultivo:eliminar", "avance:crear", "fotografia:eliminar"];

    public ProcesarSincronizacionValidator()
    {
        RuleFor(x => x.IdDispositivo).NotNull().WithMessage("El dispositivo es obligatorio.");

        RuleFor(x => x.Operaciones)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("No hay operaciones para sincronizar.")
            .Must(o => o.Count <= MaximoOperaciones)
            .WithMessage($"Se pueden enviar máximo {MaximoOperaciones} operaciones por lote.");

        RuleForEach(x => x.Operaciones).ChildRules(operacion =>
        {
            operacion.RuleFor(o => o.IdLocal).NotNull().WithMessage("Cada operación necesita su idLocal.");
            operacion.RuleFor(o => $"{o.Entidad}:{o.Operacion}")
                .Must(Permitidas.Contains)
                .WithMessage("La operación no se puede sincronizar.")
                .OverridePropertyName("operacion");
        });
    }
}