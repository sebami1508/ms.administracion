namespace Comun.Dto
{
    public class ProductoExcelImportResultDto
    {
        public int TotalFilas { get; set; }
        public int Actualizados { get; set; }
        public int SinCambios { get; set; }
        public int NoEncontrados { get; set; }
        public int FilasInvalidas { get; set; }
    }
}
