namespace Comun.Dto.DtoParameter
{
    /// <summary>
    /// Registro de cliente desde la app móvil (autoservicio).
    /// </summary>
    public class CRegistroClienteDto
    {
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public decimal? Identificacion { get; set; }
        public string? Celular { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Direccion { get; set; }
        public string? Password { get; set; }
        public string? CodigoOtp { get; set; }
    }
}
