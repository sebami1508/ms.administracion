using Comun.Dto.DtoParameter;

namespace Comun.Dto.DtoReader
{
    public class ROrdenDto
    {
        public string? OrdenId { get; set; }
        public int? CantidadItem { get; set; }
        public decimal? Total { get; set; }
        public string? UsuarioId { get; set; }
        public string? UsuarioIdStr { get; set; }
        public string? EstadoId { get; set; }
        public string? EstadoIdStr { get; set; }
        public int? Mesa { get; set; }
        public List<RItemDto>? Productos { get; set; } = new List<RItemDto>();
        public DateTime? FechaRegistro { get; set; }
        public string? Codigo { get; set; }
    }
}
