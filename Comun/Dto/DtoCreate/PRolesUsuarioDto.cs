using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class PRolesUsuarioDto : GestionAuditoriaDto
    {
        public List<string> Roles { get; set; } = null!;
    }
}
