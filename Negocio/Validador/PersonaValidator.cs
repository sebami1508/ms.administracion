using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CPersonaValidator : AbstractValidator<CPersonaDto>
    {
        public CPersonaValidator()
        {
            RuleFor(c => c.Nombres)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombres)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Nombres)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.Apellidos)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Apellidos)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Apellidos)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.TipoDocumentoId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.TipoDocumentoId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.TipoDocumentoId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.NumeroIdentificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} debe ser mayor a 0.");

            RuleFor(c => c.FechaExpedicion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.FechaExpedicion)} es obligatorio.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage(c => $"El campo {nameof(c.FechaExpedicion)} no puede ser futuro.");

            RuleFor(c => c.Correo)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Correo)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.Correo)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Correo)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.Telefono)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Telefono)} es obligatorio.")
                .MaximumLength(15).WithMessage(c => $"El campo {nameof(c.Telefono)} debe tener máximo 15 caracteres.");

            RuleFor(c => c.Direccion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Direccion)} es obligatorio.")
                .MaximumLength(200).WithMessage(c => $"El campo {nameof(c.Direccion)} debe tener máximo 200 caracteres.");

            RuleFor(c => c.FechaNacimiento)
                .NotNull().WithMessage(c => $"El campo {nameof(c.FechaNacimiento)} es obligatorio.")
                .LessThan(DateTime.Today).WithMessage(c => $"El campo {nameof(c.FechaNacimiento)} debe ser en el pasado.");

            RuleFor(c => c.GeneroId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.GeneroId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.GeneroId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.TerminosCondiciones)
                .Equal(true).WithMessage(c => $"Debe aceptar {nameof(c.TerminosCondiciones)}.");

            RuleFor(c => c.PoliticaTratamientoDatos)
                .Equal(true).WithMessage(c => $"Debe aceptar {nameof(c.PoliticaTratamientoDatos)}.");

            RuleFor(c => c.MayorEdad)
                .Equal(true).WithMessage(c => $"Debe confirmar que es {nameof(c.MayorEdad)}.");

            RuleFor(c => c.ResponsabilidadFiscalId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.ResponsabilidadFiscalId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.ResponsabilidadFiscalId)} debe tener entre 1 y 40 caracteres.");
        }
    }

    public class UPersonaValidator : AbstractValidator<UPersonaDto>
    {
        public UPersonaValidator()
        {
            RuleFor(c => c.PersonaId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.PersonaId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.PersonaId)} debe tener entre 1 y 40 caracteres.");

            // Reutilizar reglas de creación donde aplican
            RuleFor(c => c.Nombres)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Nombres)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Nombres)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.Apellidos)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Apellidos)} es obligatorio.")
                .Length(1, 150).WithMessage(c => $"El campo {nameof(c.Apellidos)} debe tener entre 1 y 150 caracteres.");

            RuleFor(c => c.TipoDocumentoId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.TipoDocumentoId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.TipoDocumentoId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.NumeroIdentificacion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} es obligatorio.")
                .GreaterThan(0).WithMessage(c => $"El campo {nameof(c.NumeroIdentificacion)} debe ser mayor a 0.");

            RuleFor(c => c.FechaExpedicion)
                .NotNull().WithMessage(c => $"El campo {nameof(c.FechaExpedicion)} es obligatorio.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage(c => $"El campo {nameof(c.FechaExpedicion)} no puede ser futuro.");

            RuleFor(c => c.Correo)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Correo)} es obligatorio.")
                .EmailAddress().WithMessage(c => $"El campo {nameof(c.Correo)} debe tener formato de correo válido.")
                .MaximumLength(150).WithMessage(c => $"El campo {nameof(c.Correo)} debe tener máximo 150 caracteres.");

            RuleFor(c => c.Telefono)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Telefono)} es obligatorio.")
                .MaximumLength(15).WithMessage(c => $"El campo {nameof(c.Telefono)} debe tener máximo 15 caracteres.");

            RuleFor(c => c.Direccion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Direccion)} es obligatorio.")
                .MaximumLength(200).WithMessage(c => $"El campo {nameof(c.Direccion)} debe tener máximo 200 caracteres.");

            RuleFor(c => c.FechaNacimiento)
                .NotNull().WithMessage(c => $"El campo {nameof(c.FechaNacimiento)} es obligatorio.")
                .LessThan(DateTime.Today).WithMessage(c => $"El campo {nameof(c.FechaNacimiento)} debe ser en el pasado.");

            RuleFor(c => c.GeneroId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.GeneroId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.GeneroId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.ResponsabilidadFiscalId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.ResponsabilidadFiscalId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.ResponsabilidadFiscalId)} debe tener entre 1 y 40 caracteres.");
        }
    }
}
