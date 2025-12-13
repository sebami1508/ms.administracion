namespace Datos.Orm.Entidades
{
    public class TaLegalModel
    {
        public string RegistroId { get; set; } = null!;
        public string TipoLoteriaId { get; set; } = null!;
        public string CiudadId { get; set; } = null!;
        public decimal NumeroBillete { get; set; }
        public decimal NumeroSerie { get; set; }
        public decimal NumeroSorteo { get; set; }
        public string? DistribuidorId { get; set; }
        public string EstadoId { get; set; } = null!;
        public string? Observacion { get; set; }
        public bool Vigente { get; set; }
        public string UsuarioId { get; set; } = null!;

        public TaDominioModel TaDominioModelTipoLoteriaId { get; set; } = null!;
        public TaDominioModel TaDominioModelEstadoId { get; set; } = null!;
        public TaDistribuidorModel? TaDistribuidorModel { get; set; }
        public TaUsuarioModel TaUsuarioModel { get; set; } = null!;
    }
}
