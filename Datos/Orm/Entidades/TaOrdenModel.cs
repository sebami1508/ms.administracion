namespace Datos.Orm.Entidades
{
    public class TaOrdenModel
    {
        public string OrdenId { get; set; } = null!;
        public int CantidadItem { get; set; }
        public decimal Total { get; set; }
        public string UsuarioId { get; set; } = null!;
        public bool Vigente { get; set; }
        public string EstadoId { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }
        public int? Mesa { get; set; }
        public string Codigo { get; set; } = null!;
        public string? MetodoPagoId { get; set; }
        public bool Domicilio { get; set; }
        public string? Cliente { get; set; }
        public string? Direccion { get; set; }
        public string? NumeroFactura { get; set; }
        public decimal? TotalTransferencia { get; set; }
        public decimal? TotalEfectivo { get; set; }
        public string TurnoId { get; set; } = null!;

        public TaUsuarioModel TaUsuarioModel { get; set; }
        public TaDominioModel TaDominioModel { get; set; }
        public TaDominioModel TaDominioModel2 { get; set; }
        public TaTurnoModel TaTurnoModel { get; set; }
        public ICollection<TaItemModel> LtsTaItemModel { get; set; } = new List<TaItemModel>();
    }
}
