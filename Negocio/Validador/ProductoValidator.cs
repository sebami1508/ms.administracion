using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CProductoValidator : AbstractValidator<CProductoDto>
    {
        public CProductoValidator()
        {
            RuleFor(x => x.CategoriaId).NotEmpty();
            RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Precio).NotNull().GreaterThanOrEqualTo(0);
        }
    }

    public class UProductoValidator : AbstractValidator<UProductoDto>
    {
        public UProductoValidator()
        {
            RuleFor(x => x.ProductoId).NotEmpty();
            RuleFor(x => x.Descripcion).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
            RuleFor(x => x.Precio).GreaterThanOrEqualTo(0).When(x => x.Precio.HasValue);
        }
    }
}
