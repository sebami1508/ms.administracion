namespace Datos.Orm.Entidades
{
    public class TaDominioModel
    {
        #region Propiedades

        public string DominioId { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }

        public TaDominioModel? Padre { get; set; }
        public ICollection<TaDominioModel> LtsHijos { get; set; } = new List<TaDominioModel>();
        public ICollection<TaProductoModel> LtsTaProductoModel { get; set; } = new List<TaProductoModel>();
        public ICollection<TaOrdenModel> LtsTaOrdenModel { get; set; } = new List<TaOrdenModel>();
        public ICollection<TaOrdenModel> LtsTaOrdenModel2 { get; set; } = new List<TaOrdenModel>();
        public ICollection<TaPizzaModel> LtsTaPizzaModelTipo { get; set; } = new List<TaPizzaModel>();
        public ICollection<TaPizzaModel> LtsTaPizzaModelSabor { get; set; } = new List<TaPizzaModel>();

        #endregion
    }
}
