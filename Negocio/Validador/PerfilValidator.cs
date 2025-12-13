using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CPerfilValidator : AbstractValidator<CPerfilDto>
    {
        public CPerfilValidator()
        {
            RuleFor(c => c.MenuId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.MenuId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.MenuId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.RolId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolId)} debe tener entre 1 y 40 caracteres.");
        }
    }

    public class UPerfilValidator : AbstractValidator<UPerfilDto>
    {
        public UPerfilValidator()
        {
            RuleFor(c => c.PerfilId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.PerfilId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.PerfilId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.MenuId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.MenuId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.MenuId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.RolId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolId)} debe tener entre 1 y 40 caracteres.");
        }
    }
}
