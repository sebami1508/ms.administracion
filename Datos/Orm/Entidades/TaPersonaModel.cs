namespace Datos.Orm.Entidades
{
    public class TaPersonaModel
    {
        #region Propiedades

        public string PersonaId { get; set; } = null!;
        public string Nombres { get; set; } = null!;
        public string Apellidos { get; set; } = null!;
        public string TipoDocumentoId { get; set; } = null!;
        public decimal NumeroIdentificacion { get; set; }
        public DateTime FechaExpedicion { get; set; }
        public string Correo { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public DateTime FechaNacimiento { get; set; }
        public string GeneroId { get; set; } = null!;
        public bool TerminosCondiciones { get; set; }
        public bool PoliticaTratamientoDatos { get; set; }
        public bool MayorEdad { get; set; }
        public string ResponsabilidadFiscalId { get; set; } = null!;


        public virtual TaDominioModel? TaDominioModelTipoDocumento { get; set; }
        public virtual TaDominioModel? TaDominioModelGenero { get; set; }
        public virtual TaDominioModel? TaDominioModelResponsabilidad { get; set; }

        #endregion
    }
}
