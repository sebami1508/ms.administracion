using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CItemDto
    {
        public string? OrdenId { get; set; }
        public string ProductoId { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string? NombrePlato { get; set; }

        public List<CCaracteristicasDto>? Caracteristicas { get; set; } = new List<CCaracteristicasDto>();
        
    }
}
