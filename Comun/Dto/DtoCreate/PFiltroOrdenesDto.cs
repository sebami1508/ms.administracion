
using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class PFiltroOrdenesDto : GestionAuditoriaDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
