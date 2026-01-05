using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CDominioValidator : AbstractValidator<CDominioDto>
    {
        public CDominioValidator()
        {
            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 200).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 200 caracteres.");

            RuleFor(c => c.PadreId)
                .MaximumLength(40).WithMessage(c => $"El campo {nameof(c.PadreId)} debe tener máximo 40 caracteres.");
        }
    }

    public class UDominioValidator : AbstractValidator<UDominioDto>
    {
        public UDominioValidator()
        {
            RuleFor(c => c.DominioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.DominioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.DominioId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 200).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 200 caracteres.");

            RuleFor(c => c.PadreId)
                .MaximumLength(40).WithMessage(c => $"El campo {nameof(c.PadreId)} debe tener máximo 40 caracteres.");
        }
    }
}
