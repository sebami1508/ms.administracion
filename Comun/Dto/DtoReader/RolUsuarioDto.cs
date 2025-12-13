namespace Comun.Dto.DtoParameter
{
    public class RRolUsuarioDto
    {
        public string? RolUsuarioId { get; set; }
        public string? UsuarioId { get; set; }
        public string? RolId { get; set; }


        #region Propiedades de consulta
        public string? DescripcionRol { get; set; }
        #endregion

    }
}
