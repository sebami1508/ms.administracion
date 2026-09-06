using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class COrdenDto
    {
        public string Codigo { get; set; } = null!;
        public int CantidadItem { get; set; }
        public decimal Total { get; set; }
        public string UsuarioId { get; set; } = null!;
        public int? Mesa { get; set; }
        public bool Domicilio { get; set; }
        public string? Cliente { get; set; }
        public string? Direccion { get; set; }
        public string TurnoId { get; set; } = null!;

        /// <summary>
        /// Estado inicial opcional. Si no se envía, la orden se crea como
        /// Pendiente. Las órdenes creadas por clientes desde la App envían
        /// el estado "Por validar".
        /// </summary>
        public string? EstadoId { get; set; }

        public List<CItemDto> Productos { get; set; } = new List<CItemDto>();
        
    }
}
