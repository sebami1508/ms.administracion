using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class SolicitarOtpResetDto : GestionAuditoriaDto
    {
        public string IdentificacionOCorreo { get; set; } = null!;
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
    }

}
