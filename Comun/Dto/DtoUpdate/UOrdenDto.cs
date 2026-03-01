namespace Comun.Dto.DtoParameter
{
    public class UOrdenDto
    {
        public string OrdenId { get; set; } = null!;
        public string EstadoId { get; set; } = null!;
        public int? CantidadItem { get; set; }
        public decimal? Total { get; set; }
        public string? MetodoPagoId { get; set; }
        public decimal? TotalTransferencia { get; set; }
        public decimal? TotalEfectivo { get; set; }
    }
}
