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
        public DateTime? FechaRegistro { get; set; }
        public string? Codigo { get; set; }
        public bool Domicilio { get; set; }
        public string? Cliente { get; set; }
        public string? Direccion { get; set; }
        public string? MetodoPagoId { get; set; }
        public string? MetodoPagoIdStr { get; set; }
        public List<RItemDto>? Productos { get; set; } = new List<RItemDto>();

    }
}
