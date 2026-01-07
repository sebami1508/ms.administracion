using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CPizzaDto : GestionAuditoriaDto
    {
        public string? PizzaId { get; set; }
        public string? ItemId { get; set; }
        public string TipoId { get; set; } = null!;
        public string SaborId { get; set; } = null!;
    }
}
