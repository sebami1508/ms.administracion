namespace Datos.Orm.Entidades
{
    public class TaUsuarioModel
    {
        #region Propiedades

        public string UsuarioId { get; set; }
        public string Nombres { get; set; }
        public string? Apellidos { get; set; }
        public decimal Identificacion { get; set; }
        public string Celular { get; set; }
        public string CorreoElectronico { get; set; }
        public string Password { get; set; }
        public bool IngresoPrimeraVez { get; set; }
        public bool Vigente { get; set; }
        public bool? Externo { get; set; }


        public ICollection<TaRolUsuarioModel> LtsTaRolesModel { get; set; }
        public ICollection<TaAuditoriaModel> LtsTaAuditoria { get; set; }

        #endregion
    }
}
