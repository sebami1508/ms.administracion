using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CTurnoValidator : AbstractValidator<CTurnoDto>
    {
        public CTurnoValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.UsuarioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.UsuarioId)} debe tener entre 1 y 200 caracteres.");

            RuleFor(c => c.FechaTurno)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.FechaTurno)} es obligatorio.");

            RuleFor(c => c.Base)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Base)} es obligatorio.");
        }
    }

    public class UTurnoValidator : AbstractValidator<UTurnoDto>
    {
        public UTurnoValidator()
        {
            RuleFor(c => c.TurnoId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.TurnoId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.TurnoId)} debe tener entre 1 y 40 caracteres.");
        }
    }
}
