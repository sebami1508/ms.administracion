namespace Datos.Orm.Entidades
{
    public class TaDistribuidorModel
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


        public virtual TaDominioModel TaDominioModel { get; set; }
    }
}
