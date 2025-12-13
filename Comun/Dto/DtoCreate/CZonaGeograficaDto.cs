using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoCreate
{
    public class CZonaGeograficaDto : GestionAuditoriaDto
    {
        public string? Descripcion { get; set; }
        public decimal? CodigoDane { get; set; }
        public string? Longitud { get; set; }
        public string? Latitud { get; set; }
        public string? PadreId { get; set; }
        public string? CodigoIso { get; set; }
    }
}
