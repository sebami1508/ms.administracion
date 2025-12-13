namespace Comun.Dto.DtoParameter
{
    public class RDistribuidorDto
    {
        public string? DistribuidorId { get; set; }
        public string? Nombre { get; set; }
        public string? TipoIdentificacionId { get; set; }
        public decimal? NumeroIdentificacion { get; set; }
        public string? Direccion { get; set; }
        public string? PersonaContacto { get; set; }
        public decimal? Telefono { get; set; }
        public string? Correo { get; set; }
        public bool? Vigente { get; set; }

        public string? TipoIdentificacionStr { get; set; }
    }
}
