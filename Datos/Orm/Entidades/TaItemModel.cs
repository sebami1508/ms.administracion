namespace Datos.Orm.Entidades
{
    public class TaItemModel
    {
        public string ItemId { get; set; } = null!;
        public string OrdenId { get; set; } = null!;
        public string ProductoId { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string? NombrePlato { get; set; }

        public TaOrdenModel TaOrdenModel { get; set; }
        public TaProductoModel TaProductoModel { get; set; }
        public ICollection<TaCaracteristicaModel> LtsTaCaracteristicaModel { get; set; }
    }
}
