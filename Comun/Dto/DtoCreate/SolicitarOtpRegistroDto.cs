namespace Comun.Dto.DtoParameter
{
    /// <summary>
    /// Solicitud de código OTP para verificar el correo durante el registro.
    /// </summary>
    public class SolicitarOtpRegistroDto
    {
        public string? CorreoElectronico { get; set; }
        public string? Nombres { get; set; }
    }
}
