using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UOrdenDto : GestionAuditoriaDto
    {
        public string OrdenId { get; set; } = null!;
        public int? CantidadItem { get; set; }
        public decimal? Total { get; set; }
        public string EstadoId { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public string? MetodoPagoId { get; set; }
    }
}
