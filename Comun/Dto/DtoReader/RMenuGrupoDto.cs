namespace Comun.Dto.DtoParameter
{
    public class RMenuGrupoDto
    {
        public string? MenuId { get; set; }
        public string? PerfilId { get; set; }
        public string? Nombre { get; set; }
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public decimal Orden { get; set; }
        public bool Vigente { get; set; }
        public List<RMenuDto> SubMenus { get; set; } = new();
    }
}
