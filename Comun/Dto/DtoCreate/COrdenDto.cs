using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class COrdenDto : GestionAuditoriaDto
    {
        public string? OrdenId { get; set; }
        public string Codigo { get; set; } = null!;
        public int CantidadItem { get; set; }
        public decimal Total { get; set; }
        public string UsuarioId { get; set; } = null!;
        public string? EstadoId { get; set; }
        public int? Mesa { get; set; }
        public string? MetodoPagoId { get; set; }
        public bool Domicilio { get; set; }
        public string? Cliente { get; set; }
        public string? Direccion { get; set; }

        public List<CItemDto> Productos { get; set; } = new List<CItemDto>();
        
    }
}
