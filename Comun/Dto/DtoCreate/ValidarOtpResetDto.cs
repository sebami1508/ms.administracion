using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class ValidarOtpResetDto: GestionAuditoriaDto
    {
        public string IdentificacionOCorreo { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }

}
