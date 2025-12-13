using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class UPersonaDto : GestionAuditoriaDto
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
        public string? Password { get; set; }
    }
}
