namespace Datos.Orm.Entidades
{
    public class TaMenuCartaModel
    {
        #region Propiedades

        public string MenuCartaId { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public string Contenido { get; set; } = null!; // PDF en Base64
        public DateTime FechaRegistro { get; set; }
        public bool Vigente { get; set; }

        #endregion
    }
}
