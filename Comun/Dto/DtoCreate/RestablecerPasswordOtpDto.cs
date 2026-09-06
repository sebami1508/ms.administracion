namespace Comun.Dto.DtoParameter
{
    public class RestablecerPasswordOtpDto
    {
        public string IdentificacionOCorreo { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
