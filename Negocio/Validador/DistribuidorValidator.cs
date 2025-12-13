using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CDistribuidorValidator : AbstractValidator<CDistribuidorDto>
    {
        public CDistribuidorValidator()
        {
            RuleFor(c => c.Nombre)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombre)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Nombre)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.TipoIdentificacionId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.TipoIdentificacionId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.TipoIdentificacionId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.NumeroIdentificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} debe ser mayor a 0.");

            RuleFor(c => c.Direccion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Direccion)} es obligatorio.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Direccion)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.PersonaContacto)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.PersonaContacto)} es obligatorio.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.PersonaContacto)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.Telefono)
               .NotNull().WithMessage(c => $"El campo {nameof(c.Telefono)} es obligatorio.")
               .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.Telefono)} debe ser mayor a 0.");

            RuleFor(c => c.Correo)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Correo)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.Correo)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Correo)} debe tener máximo 150 caracteres.");
        }
    }

    public class UDistribuidorValidator : AbstractValidator<UDistribuidorDto>
    {
        public UDistribuidorValidator()
        {
            RuleFor(c => c.DistribuidorId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.DistribuidorId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.DistribuidorId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.Nombre)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombre)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Nombre)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.TipoIdentificacionId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.TipoIdentificacionId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.TipoIdentificacionId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.NumeroIdentificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} debe ser mayor a 0.");

            RuleFor(c => c.Direccion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Direccion)} es obligatorio.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Direccion)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.PersonaContacto)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.PersonaContacto)} es obligatorio.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.PersonaContacto)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.Telefono)
              .NotNull().WithMessage(c => $"El campo {nameof(c.Telefono)} es obligatorio.")
              .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.Telefono)} debe ser mayor a 0.");

            RuleFor(c => c.Correo)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Correo)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.Correo)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Correo)} debe tener máximo 150 caracteres.");
        }
    }
}
