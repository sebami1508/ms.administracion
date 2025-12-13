namespace Comun.Dto.DtoParameter
{
    public class RPersonaDto
    {
        public string? PersonaId { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? TipoDocumentoId { get; set; }
        public decimal? NumeroIdentificacion { get; set; }
        public DateTime? FechaExpedicion { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? GeneroId { get; set; }
        public bool TerminosCondiciones { get; set; }
        public bool PoliticaTratamientoDatos { get; set; }
        public bool MayorEdad { get; set; }
        public string? ResponsabilidadFiscalId { get; set; }


        #region Porpiedades de navegación
        public string? GeneroIdStr { get; set; }
        public string? TipoDocumentoIdStr { get; set; }
        public string? ResponsabilidadFiscalIdStr { get; set; }
        #endregion
    }
}
