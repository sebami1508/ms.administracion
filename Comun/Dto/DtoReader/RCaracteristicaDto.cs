namespace Comun.Dto.DtoParameter
{
    public class RCaracteristicaDto
    {
        public string CaracteristicaId { get; set; } = null!;
        public string ItemId { get; set; } = null!;
        public bool? UnSabor { get; set; }
        public bool? EnPatacon { get; set; }
        public string? Observacion { get; set; }
    }
}
