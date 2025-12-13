namespace Comun.Dto.DtoParameter
{
    public class RDominioDto
    {
        public string? DominioId { get; set; } 
        public string? Descripcion { get; set; }
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }


        #region Propiedades de consulta
        public string? PadreIdStr { get; set; }
        #endregion


    }
}
