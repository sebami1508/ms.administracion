using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class URolDto : GestionAuditoriaDto
    {
        public string? RolId { get; set; }
        public string? Descripcion { get; set; }
        public bool? Vigente { get; set; }
    }
}
