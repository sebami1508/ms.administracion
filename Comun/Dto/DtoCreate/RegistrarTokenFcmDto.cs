namespace Comun.Dto.DtoParameter
{
    public class RegistrarTokenFcmDto
    {
        public string UsuarioId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string? Plataforma { get; set; }
    }
}
