namespace Datos.Orm.Entidades
{
    public class TaCaracteristicaModel
    {
        #region Propiedades

        public string CaracteristicaId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public bool? UnSabor { get; set; }
        public bool? EnPatacon { get; set; }
        public string? Observacion { get; set; }


        public TaItemModel TaItemModel { get; set; } = null!;

        #endregion
    }
}
