using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CDominioDto : GestionAuditoriaDto
    {
        public string? Descripcion { get; set; }
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }

    }
}
