using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CRolDto : GestionAuditoriaDto
    {
        public string? Descripcion { get; set; }
        public bool? Vigente { get; set; }
    }
}
