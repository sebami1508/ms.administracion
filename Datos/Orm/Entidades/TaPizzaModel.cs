namespace Datos.Orm.Entidades
{
    public class TaPizzaModel
    {
        #region Propiedades

        public string PizzaId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public string TipoId { get; set; } = null!;
        public string SaborId { get; set; } = null!;


        public TaItemModel TaItemModel { get; set; }
        public TaDominioModel TaDominioModelTipo { get; set; }
        public TaDominioModel TaDominioModelSabor { get; set; }

        #endregion
    }
}
