namespace Comun.Dto.DtoParameter
{
    public class UTurnoDto
    {
        public string TurnoId { get; set; } = null!;
        public string? EstadoId { get; set; }
        public decimal? Base { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Finalizado { get; set; }
    }
}
