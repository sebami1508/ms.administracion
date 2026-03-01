using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UDominioDto
    {
        public string? DominioId { get; set; } 
        public string? Descripcion { get; set; }
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }

    }
}
