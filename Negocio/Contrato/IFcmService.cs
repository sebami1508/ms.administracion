namespace Negocio.Contrato
{
    /// <summary>
    /// Servicio de notificaciones push (Firebase Cloud Messaging).
    /// </summary>
    public interface IFcmService
    {
        /// <summary>Registra/actualiza el token FCM de un dispositivo para un usuario.</summary>
        Task RegistrarTokenAsync(string usuarioId, string token, string? plataforma);

        /// <summary>Envía una notificación push a todos los dispositivos del usuario.</summary>
        Task EnviarAsync(string usuarioId, string titulo, string cuerpo,
            Dictionary<string, string>? data = null);
    }
}
