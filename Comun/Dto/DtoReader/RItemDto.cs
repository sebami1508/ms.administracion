using Comun.Dto.DtoParameter;

namespace Comun.Dto.DtoReader
{
    public class RItemDto
    {
        public string ItemId { get; set; }
        public string OrdenId { get; set; }
        public string ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string ProductoDescripcion { get; set; }
        public string? CategoriaId { get; set; }
        public string? CategoriaIdStr { get; set; }
        public List<RPizzaDto>? Caracteristicas { get; set; } = new List<RPizzaDto>();
    }
}
