using System.ComponentModel.DataAnnotations.Schema;

namespace Datos.Orm.Entidades
{
    public class TaRolUsuarioModel
    {
        #region Propiedades

        public string RolUsuarioId { get; set; }
        public string UsuarioId { get; set; }
        public string RolId { get; set; }
        


        public virtual TaRolModel TaRolModel { get; set; }
        public virtual TaUsuarioModel TaUsuarioModel { get; set; }

        #endregion
    }
}
