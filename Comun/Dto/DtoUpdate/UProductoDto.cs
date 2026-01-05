using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UProductoDto : GestionAuditoriaDto
    {
        public string? ProductoId { get; set; }
        public string? CategoriaId { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
    }
}
