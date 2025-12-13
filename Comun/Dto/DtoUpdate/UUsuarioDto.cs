using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UUsuarioDto : GestionAuditoriaDto
    {
        public string? UsuarioId { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public decimal? Identificacion { get; set; }
        public string? Celular { get; set; }
        public string? CorreoElectronico { get; set; }
        public bool Vigente { get; set; }
        public string? Password { get; set; }
        public bool IngresoPrimeraVez { get; set; }
        public bool? Externo { get; set; }


        #region Propiedades de consulta
        public int? User { get; set; }
        public string? NewPassword1 { get; set; }
        public string? NewPassword2 { get; set; }
        #endregion
    }
}
