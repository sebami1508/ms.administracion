using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CItemDto : GestionAuditoriaDto
    {
        public string OrdenId { get; set; } = null!;
        public string ProductoId { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
