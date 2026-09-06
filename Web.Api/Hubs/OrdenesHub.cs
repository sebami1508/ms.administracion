using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Api.Hubs
{
    /// <summary>
    /// Hub de órdenes en tiempo real.
    ///
    /// Como el JWT del sistema es un token de servicio genérico (no lleva la
    /// identidad real del usuario), la pertenencia a grupos se define con datos
    /// que el cliente envía por query string al conectar:
    ///   - personal=true  -> se une al grupo "personal" (staff: recibe órdenes
    ///                        nuevas y todos los cambios de estado).
    ///   - usuarioId={id}  -> se une al grupo "usuario:{id}" (cliente: recibe los
    ///                        cambios de estado de SUS órdenes).
    ///
    /// Eventos que emite el servidor:
    ///   - "OrdenNueva"            (payload de la orden)
    ///   - "OrdenEstadoCambiado"   (payload de la orden)
    /// </summary>
    [Authorize]
    public class OrdenesHub : Hub
    {
        public const string GrupoPersonal = "personal";

        public static string GrupoUsuario(string usuarioId) => $"usuario:{usuarioId}";

        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            var query = http?.Request.Query;

            var esPersonal = string.Equals(query?["personal"], "true", StringComparison.OrdinalIgnoreCase);
            if (esPersonal)
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoPersonal);

            var usuarioId = query?["usuarioId"].ToString();
            if (!string.IsNullOrWhiteSpace(usuarioId))
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUsuario(usuarioId.Trim()));

            await base.OnConnectedAsync();
        }
    }
}
