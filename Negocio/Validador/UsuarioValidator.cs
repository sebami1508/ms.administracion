using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CUsuarioValidator : AbstractValidator<CUsuarioDto>
    {
        public CUsuarioValidator()
        {
            RuleFor(c => c.Nombres)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombres)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Nombres)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.Apellidos)
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Apellidos)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.Identificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.Identificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.Identificacion)} debe ser mayor a 0.");

            RuleFor(c => c.Celular)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Celular)} es obligatorio.")
                .Length(10).WithMessage(c => $"El campo {nameof(c.Celular)} debe tener 10 caracteres.");

            RuleFor(c => c.CorreoElectronico)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} debe tener máximo 150 caracteres.");
        }
    }

    public class UUsuarioValidator : AbstractValidator<UUsuarioDto>
    {
        public UUsuarioValidator()
        {
            RuleFor(c => c.UsuarioId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.UsuarioId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.UsuarioId)} debe tener entre 1 y 40 caracteres.");

            // Reglas de creación
            RuleFor(c => c.Nombres)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombres)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Nombres)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.Apellidos)
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Apellidos)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.Identificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.Identificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.Identificacion)} debe ser mayor a 0.");

            RuleFor(c => c.Celular)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Celular)} es obligatorio.")
                .Length(10).WithMessage(c => $"El campo {nameof(c.Celular)} debe tener 10 caracteres.");

            RuleFor(c => c.CorreoElectronico)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.CorreoElectronico)} debe tener máximo 150 caracteres.");
        }
    }
}
