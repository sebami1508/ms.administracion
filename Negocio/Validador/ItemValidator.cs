using Comun.Dto.DtoParameter;
using FluentValidation;

namespace Negocio.Validador
{
    public class CItemValidator : AbstractValidator<CItemDto>
    {
        public CItemValidator()
        {
            RuleFor(x => x.OrdenId).NotEmpty();
            RuleFor(x => x.ProductoId).NotEmpty();
            RuleFor(x => x.Cantidad).NotNull().GreaterThan(0);
            RuleFor(x => x.Subtotal).NotNull().GreaterThanOrEqualTo(0);
        }
    }

    public class UItemValidator : AbstractValidator<UItemDto>
    {
        public UItemValidator()
        {
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.Cantidad).GreaterThan(0).When(x => x.Cantidad.HasValue);
            RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0).When(x => x.Subtotal.HasValue);
        }
    }
}
