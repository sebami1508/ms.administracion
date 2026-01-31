using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CCaracteristicasDto : GestionAuditoriaDto
    {
        public bool? UnSabor { get; set; }
        public bool? EnPatacon { get; set; }
        public string? Observacion { get; set; }
    }
}
