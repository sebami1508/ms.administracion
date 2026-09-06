namespace Datos.Orm.Entidades
{
    /// <summary>
    /// Token de dispositivo (FCM) asociado a un usuario, para enviar
    /// notificaciones push a la aplicación móvil.
    /// </summary>
    public class TaDispositivoFcmModel
    {
        public Guid DispositivoId { get; set; }
        public string UsuarioId { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string? Plataforma { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
