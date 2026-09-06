namespace Datos.Orm.Entidades
{
    /// <summary>
    /// OTP de verificación de correo para el registro de clientes
    /// (previo a la creación del usuario, por eso no tiene FK a TA_USUARIO).
    /// </summary>
    public class TaOtpRegistroModel
    {
        public Guid OtpRegistroId { get; set; }
        public string Correo { get; set; } = null!;

        public string OtpHash { get; set; } = null!;
        public string OtpSalt { get; set; } = null!;

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; }
        public DateTime? FechaUso { get; set; }

        public int Intentos { get; set; }
        public int MaxIntentos { get; set; }
    }
}
