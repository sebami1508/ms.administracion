namespace Datos.Orm.Entidades
{
    public class TaDominioModel
    {
        #region Propiedades

        public string DominioId { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string? PadreId { get; set; }
        public bool Vigente { get; set; }

        public virtual TaDominioModel? Padre { get; set; }
        public virtual ICollection<TaDominioModel> LtsHijos { get; set; } = new List<TaDominioModel>();
        public virtual ICollection<TaPersonaModel> LtsTaPersonaModelTipoDocumento { get; set; } = new List<TaPersonaModel>();
        public virtual ICollection<TaPersonaModel> LtsTaPersonaModelGenero { get; set; } = new List<TaPersonaModel>();
        public virtual ICollection<TaPersonaModel> LtsTaPersonaModelResponsabilidad { get; set; } = new List<TaPersonaModel>();
        public virtual ICollection<TaDistribuidorModel> LtsTaDistribuidorModel { get; set; } = new List<TaDistribuidorModel>();

        #endregion
    }
}
