namespace Comun.Dto.DtoParameter
{
    public class RTurnoDto
    {
        public string TurnoId { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;
        public DateTime FechaTurno { get; set; }
        public string EstadoId { get; set; } = null!;
        public decimal Base { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }





        #region Propiedades de Consulta

        public string? EstadoStr { get; set; }
        public string? UsuarioStr { get; set; }

        #endregion
    }
}
