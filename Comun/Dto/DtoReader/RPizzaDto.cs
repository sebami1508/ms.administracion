using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class RPizzaDto
    {
        public string PizzaId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public string TipoId { get; set; } = null!;
        public string SaborId { get; set; } = null!;
        public string? TipoIdStr { get; set; }
        public string? SaborIdStr { get; set; }
    }
}
