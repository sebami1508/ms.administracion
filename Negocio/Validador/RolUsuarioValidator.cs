using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CRolUsuarioValidator : AbstractValidator<CRolUsuarioDto>
    {
        public CRolUsuarioValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.UsuarioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.UsuarioId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.RolId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolId)} debe tener entre 1 y 40 caracteres.");
        }
    }

    public class URolUsuarioValidator : AbstractValidator<URolUsuarioDto>
    {
        public URolUsuarioValidator()
        {
            RuleFor(c => c.RolUsuarioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolUsuarioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolUsuarioId)} debe tener entre 1 y 40 caracteres.");

            // Reglas de creación
            RuleFor(c => c.UsuarioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.UsuarioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.UsuarioId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.RolId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.RolId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.RolId)} debe tener entre 1 y 40 caracteres.");
        }
    }
}
