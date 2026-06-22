namespace Datos.Orm.Entidades
{
    public class TaTurnoModel
    {
        #region Propiedades

        public string TurnoId { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;
        public DateTime FechaTurno { get; set; }
        public string EstadoId { get; set; } = null!;
        public decimal Base { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Finalizado { get; set; }


        public TaUsuarioModel TaUsuarioModel { get; set; } = null!;
        public TaDominioModel TaDominioModel { get; set; } = null!;
        public ICollection<TaOrdenModel> LtsTaOrdenModel { get; set; } = new List<TaOrdenModel>();
      

        #endregion
    }
}
