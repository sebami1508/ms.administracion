using Microsoft.AspNetCore.SignalR;
using Negocio.Contrato;
using Web.Api.Hubs;

namespace Web.Api.Realtime
{
    /// <summary>
    /// Implementación del notificador en tiempo real sobre SignalR.
    /// Nunca lanza excepciones al llamador: notificar es complementario y no debe
    /// romper el flujo de negocio (igual criterio que el envío de push FCM).
    /// </summary>
    public class RealtimeNotificador : IRealtimeNotificador
    {
        private const string EvtOrdenNueva = "OrdenNueva";
        private const string EvtOrdenEstadoCambiado = "OrdenEstadoCambiado";

        private readonly IHubContext<OrdenesHub> _hub;
        private readonly ILogger<RealtimeNotificador> _logger;

        public RealtimeNotificador(IHubContext<OrdenesHub> hub, ILogger<RealtimeNotificador> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public async Task OrdenNuevaAsync(object payload)
        {
            try
            {
                await _hub.Clients.Group(OrdenesHub.GrupoPersonal)
                    .SendAsync(EvtOrdenNueva, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo emitir OrdenNueva por SignalR.");
            }
        }

        public async Task OrdenEstadoCambiadoAsync(string? usuarioIdCliente, object payload)
        {
            try
            {
                await _hub.Clients.Group(OrdenesHub.GrupoPersonal)
                    .SendAsync(EvtOrdenEstadoCambiado, payload);

                if (!string.IsNullOrWhiteSpace(usuarioIdCliente))
                {
                    await _hub.Clients.Group(OrdenesHub.GrupoUsuario(usuarioIdCliente.Trim()))
                        .SendAsync(EvtOrdenEstadoCambiado, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo emitir OrdenEstadoCambiado por SignalR.");
            }
        }
    }
}
