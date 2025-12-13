namespace Datos.Orm.Entidades
{
    public class TaAuditoriaConsultaModel
    {
        public string AuditoriaConsultaId { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public string Metodo { get; set; } = null!;
        public string? Parametros { get; set; }
        public DateTime Fecha { get; set; }
        public string MaquinaIp { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;


        public TaUsuarioModel TaUsuarioModel { get; set; }

    }
}
