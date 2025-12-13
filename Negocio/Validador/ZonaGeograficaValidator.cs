using Comun.Dto.DtoCreate;
using Comun.Dto.DtoUpdate;
using FluentValidation;

namespace Negocio.Validador
{
    public class CZonaGeograficaValidator : AbstractValidator<CZonaGeograficaDto>
    {
        public CZonaGeograficaValidator()
        {
            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.CodigoDane)
                .GreaterThan(0).When(c => c.CodigoDane.HasValue).WithMessage(c => $"El campo {nameof(c.CodigoDane)} debe ser mayor a 0.");

            RuleFor(c => c.Longitud)
                .MaximumLength(25).WithMessage(c => $"El campo {nameof(c.Longitud)} debe tener máximo 25 caracteres.");

            RuleFor(c => c.Latitud)
                .MaximumLength(25).WithMessage(c => $"El campo {nameof(c.Latitud)} debe tener máximo 25 caracteres.");

            RuleFor(c => c.PadreId)
                .MaximumLength(40).WithMessage(c => $"El campo {nameof(c.PadreId)} debe tener máximo 40 caracteres.");

            RuleFor(c => c.CodigoIso)
                .MaximumLength(45).WithMessage(c => $"El campo {nameof(c.CodigoIso)} debe tener máximo 45 caracteres.");
        }
    }

    public class UZonaGeograficaValidator : AbstractValidator<UZonaGeograficaDto>
    {
        public UZonaGeograficaValidator()
        {
            RuleFor(c => c.ZonaGeograficaId)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.ZonaGeograficaId)} es obligatorio.")
                .Length(1, 40).WithMessage(c => $"El campo {nameof(c.ZonaGeograficaId)} debe tener entre 1 y 40 caracteres.");

            RuleFor(c => c.Descripcion)
                .NotEmpty().WithMessage(c => $"El campo {nameof(c.Descripcion)} es obligatorio.")
                .Length(1, 80).WithMessage(c => $"El campo {nameof(c.Descripcion)} debe tener entre 1 y 80 caracteres.");

            RuleFor(c => c.CodigoDane)
                .GreaterThan(0).When(c => c.CodigoDane.HasValue).WithMessage(c => $"El campo {nameof(c.CodigoDane)} debe ser mayor a 0.");

            RuleFor(c => c.Longitud)
                .MaximumLength(25).When(c => !string.IsNullOrEmpty(c.Longitud)).WithMessage(c => $"El campo {nameof(c.Longitud)} debe tener máximo 25 caracteres.");

            RuleFor(c => c.Latitud)
                .MaximumLength(25).When(c => !string.IsNullOrEmpty(c.Latitud)).WithMessage(c => $"El campo {nameof(c.Latitud)} debe tener máximo 25 caracteres.");

            RuleFor(c => c.PadreId)
                .MaximumLength(40).When(c => !string.IsNullOrEmpty(c.PadreId)).WithMessage(c => $"El campo {nameof(c.PadreId)} debe tener máximo 40 caracteres.");

            RuleFor(c => c.CodigoIso)
                .MaximumLength(45).When(c => !string.IsNullOrEmpty(c.CodigoIso)).WithMessage(c => $"El campo {nameof(c.CodigoIso)} debe tener máximo 45 caracteres.");
        }
    }
}
