using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class COrdenDto : GestionAuditoriaDto
    {
        public string OrdenId { get; set; } = null!;
        public int CantidadItem { get; set; }
        public decimal Total { get; set; }
        public string UsuarioId { get; set; } = null!;
        public string? EstadoId { get; set; }
        public int Mesa { get; set; }
        public List<CItemDto> Productos { get; set; } = new List<CItemDto>();
    }
}
