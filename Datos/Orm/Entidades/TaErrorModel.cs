namespace Datos.Orm.Entidades
{
    public class TaErrorModel
    {
        public string ErrorId { get; set; } = null!;
        public string Identificador { get; set; } = null!;
        public string CodigoEstado { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string Excepcion { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public string Modulo { get; set; } = null!;
        public string? InnerException { get; set; } 
        public string? StackTrace { get; set; } 
    }
}
