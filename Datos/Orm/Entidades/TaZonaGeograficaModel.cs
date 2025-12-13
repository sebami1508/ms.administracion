namespace Datos.Orm.Entidades
{
    public class TaZonaGeograficaModel
    {
        #region Propiedades

        public string ZonaGeograficaId { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public int? CodigoDane { get; set; }
        public string? Longitud { get; set; }
        public string? Latitud { get; set; }
        public string? PadreId { get; set; }
        public string? CodigoIso { get; set; }

        public virtual TaZonaGeograficaModel? Padre { get; set; }
        public virtual ICollection<TaZonaGeograficaModel> LtsZonaGeografica { get; set; } = new List<TaZonaGeograficaModel>();

        #endregion
    }
}
