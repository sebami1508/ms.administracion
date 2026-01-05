namespace Datos.Orm.Entidades
{
    public class TaProductoModel
    {
		#region Propiedades

		public string ProductoId { get; set; } = null!;
        public string CategoriaId { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
		public decimal Precio { get; set; }
		public bool Vigente { get; set; }


        public TaDominioModel TaDominioModel { get; set; } = null!;

        #endregion
    }
}
