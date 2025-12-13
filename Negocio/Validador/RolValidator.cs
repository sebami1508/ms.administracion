using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CRolValidator : AbstractValidator<CRolDto>
    {
        public CRolValidator()
        {
            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 150 caracteres.");
        }
    }

    public class URolValidator : AbstractValidator<URolDto>
    {
        public URolValidator()
        {
            RuleFor(c => c.RolId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 150 caracteres.");
        }
    }
}
