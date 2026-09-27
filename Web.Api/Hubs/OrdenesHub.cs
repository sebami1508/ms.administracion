using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Web.Api.Extension;

namespace Web.Api.Hubs
{
    /// <summary>
    /// Hub de órdenes en tiempo real.
    ///
    /// Los grupos se asignan con la identidad del JWT (ya no con lo que el
    /// cliente envía por query string):
    ///   - Personal (administradores y ventas) -> grupo "personal": recibe las
    ///     órdenes nuevas y todos los cambios de estado.
    ///   - Cualquier usuario -> grupo "usuario:{UsuarioId del token}": recibe
    ///     los cambios de estado de SUS órdenes.
    /// Los parámetros "personal" y "usuarioId" de la query se ignoran para no
    /// permitir que alguien escuche las órdenes de otra persona.
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
            var usuario = Context.User;

            if (usuario != null && usuario.EsPersonal())
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoPersonal);

            var usuarioId = usuario?.UsuarioId();
            if (!string.IsNullOrWhiteSpace(usuarioId))
                await Groups.AddToGroupAsync(Context.ConnectionId, GrupoUsuario(usuarioId.Trim()));

            await base.OnConnectedAsync();
        }
    }
}
