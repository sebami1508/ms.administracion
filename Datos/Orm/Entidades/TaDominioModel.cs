namespace Datos.Orm.Entidades
{
    public class TaDominioModel
    {
        #region Propiedades

        public string DominioId { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }

        public virtual TaDominioModel? Padre { get; set; }
        public virtual ICollection<TaDominioModel> LtsHijos { get; set; } = new List<TaDominioModel>();
        public virtual ICollection<TaProductoModel> LtsTaProductoModel { get; set; } = new List<TaProductoModel>();

        #endregion
    }
}
