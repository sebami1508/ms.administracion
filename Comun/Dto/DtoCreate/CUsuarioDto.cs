using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CUsuarioDto : GestionAuditoriaDto
    {
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public decimal? Identificacion { get; set; }
        public string? Celular { get; set; }
        public string? CorreoElectronico { get; set; }
    }
}
