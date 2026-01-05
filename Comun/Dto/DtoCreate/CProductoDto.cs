using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CProductoDto : GestionAuditoriaDto
    {
        public string? CategoriaId { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
    }
}
