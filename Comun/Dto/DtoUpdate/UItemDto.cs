using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UItemDto
    {
        public string? ItemId { get; set; }
        public int? Cantidad { get; set; }
        public decimal? Subtotal { get; set; }
        public string? Observacion { get; set; }
    }
}
