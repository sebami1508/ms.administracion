using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CItemDto : GestionAuditoriaDto
    {
        public string? OrdenId { get; set; }
        public string ProductoId { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string? TipoPizzaId { get; set; }
        public string? SaborPizzaId { get; set; }

        public List<CPizzaDto>? Caracteristicas { get; set; } = new List<CPizzaDto>();
    }
}
