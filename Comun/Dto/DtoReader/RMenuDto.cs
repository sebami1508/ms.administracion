namespace Comun.Dto.DtoParameter
{
    public class RMenuDto
    {
        public string? MenuId { get; set; }
        public string? PerfilId { get; set; }
        public string? Nombre { get; set; }
        public string? Icono { get; set; }
        public string? Ruta { get; set; }
        public string? SubMenu { get; set; }
        public bool MenuPadre { get; set; }
        public decimal Orden { get; set; }
        public bool Vigente { get; set; }
    }
}
