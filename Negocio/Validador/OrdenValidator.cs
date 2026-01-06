using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class COrdenValidator : AbstractValidator<COrdenDto>
    {
        public COrdenValidator()
        {
            RuleFor(x => x.CantidadItem).NotNull().GreaterThan(0);
            RuleFor(x => x.Total).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(x => x.UsuarioId).NotEmpty();
            RuleFor(x => x.Productos).NotEmpty().WithMessage("La orden debe contener al menos un producto.");
        }
    }

    public class UOrdenValidator : AbstractValidator<UOrdenDto>
    {
        public UOrdenValidator()
        {
            RuleFor(x => x.OrdenId).NotEmpty();
            RuleFor(x => x.EstadoId).NotEmpty().When(x => !string.IsNullOrWhiteSpace(x.EstadoId));
        }
    }
}
