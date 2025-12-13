namespace Datos.Orm.Entidades
{
    public class TaPerfilModel
    {
		#region Propiedades

		public string PerfilId { get; set; } = null!;
        public string MenuId { get; set; } = null!;
        public string RolId { get; set; } = null!;


        public virtual TaMenuModel TaMenuModel { get; set; }
        public virtual TaRolModel TaRolModel { get; set; }
  

        #endregion
    }
}
