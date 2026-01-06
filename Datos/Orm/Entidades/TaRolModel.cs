namespace Datos.Orm.Entidades
{
    public class TaRolModel
    {
		#region Propiedades

		public string RolId { get; set; }
		public string Descripcion { get; set; }
		public bool Vigente { get; set; }

        public ICollection<TaRolUsuarioModel> LtsTaRolesModel { get; set; }
        public ICollection<TaPerfilModel> LtsTaPerfilModel { get; set; }

        #endregion
    }
}
