using Comun.Dto.DtoUtilidades;

namespace Comun.Dto.DtoParameter
{
    public class CTurnoDto
    {
        public string UsuarioId { get; set; } = null!;
        public DateTime FechaTurno { get; set; }
        public decimal Base { get; set; }
    }
}
