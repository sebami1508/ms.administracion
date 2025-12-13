namespace Datos.Orm.Entidades
{
    public class TaMenuModel
    {
		#region Propiedades

		public string MenuId { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Icono { get; set; } = null!;
		public string? Ruta { get; set; }
		public bool SubMenu { get; set; } 
		public string? MenuPadre { get; set; } 
		public decimal Orden { get; set; }
		public bool Vigente { get; set; }

        public ICollection<TaPerfilModel> LtsTaPerfilModel { get; set; }

        #endregion
    }
}
