namespace Negocio.Contrato
{
    /// <summary>
    /// Notificador en tiempo real de órdenes (SignalR). La capa de Negocio solo
    /// conoce esta abstracción; la implementación concreta (con IHubContext) vive
    /// en la capa Web.Api para no filtrar dependencias de ASP.NET al dominio.
    /// </summary>
    public interface IRealtimeNotificador
    {
        /// <summary>
        /// Notifica al personal (grupo "personal") que llegó una orden nueva
        /// (típicamente "Por validar" enviada desde la App del cliente).
        /// </summary>
        Task OrdenNuevaAsync(object payload);

        /// <summary>
        /// Notifica un cambio de estado de una orden. Se envía al personal y,
        /// si se conoce, al cliente dueño de la orden (grupo "usuario:{id}").
        /// </summary>
        Task OrdenEstadoCambiadoAsync(string? usuarioIdCliente, object payload);
    }
}
