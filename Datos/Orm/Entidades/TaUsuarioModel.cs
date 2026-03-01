namespace Datos.Orm.Entidades
{
    public class TaUsuarioModel
    {
        #region Propiedades

        public string UsuarioId { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string? Apellidos { get; set; }
        public decimal Identificacion { get; set; }
        public string Celular { get; set; } = null!;
        public string CorreoElectronico { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IngresoPrimeraVez { get; set; }
        public bool Vigente { get; set; }

        public ICollection<TaRolUsuarioModel> LtsTaRolesModel { get; set; } = null!;
        public ICollection<TaOrdenModel> LtsTaOrdenModel { get; set; } = null!;
        public ICollection<TaTurnoModel> LtsTaTurnoModel { get; set; } = null!;

        #endregion
    }
}
