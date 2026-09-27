using Comun.Enumeracion;

namespace Negocio.Utilidad
{
    /// <summary>
    /// Transiciones de estado permitidas para una orden, según el rol. Es la
    /// misma regla que aplica el POS web (models/permisos-orden.ts), ahora
    /// también en el servidor para que no dependa del cliente.
    /// </summary>
    public static class ReglasOrden
    {
        private static readonly string[] CancelablesAdministrador =
            { Constantes.PorValidar, Constantes.Pendiente, Constantes.Despachado };

        private static readonly string[] CancelablesVentas =
            { Constantes.PorValidar, Constantes.Pendiente };

        /// <summary>
        /// Null si el cambio está permitido; si no, el mensaje para el usuario.
        /// </summary>
        public static string? ValidarCambioEstado(string estadoActual, string? estadoNuevo, IReadOnlyCollection<string> roles)
        {
            // Sin cambio de estado (p. ej. agregar productos o recalcular totales).
            if (string.IsNullOrWhiteSpace(estadoNuevo) || estadoNuevo.Trim() == estadoActual)
                return null;

            var nuevo = estadoNuevo.Trim();
            bool esAdministrador = roles.Contains(Constantes.RolSuperAdministrador) || roles.Contains(Constantes.RolAdministrador);
            bool esVentas = roles.Contains(Constantes.RolVentas);

            if (!esAdministrador && !esVentas)
                return "No tiene permisos para cambiar el estado de las órdenes.";

            switch (nuevo)
            {
                case Constantes.Cancelada:
                    var cancelables = esAdministrador ? CancelablesAdministrador : CancelablesVentas;
                    return cancelables.Contains(estadoActual)
                        ? null
                        : esAdministrador
                            ? "Solo se pueden cancelar órdenes por validar, pendientes o despachadas."
                            : "Su rol solo puede cancelar órdenes por validar o pendientes.";

                case Constantes.Pendiente:
                    return estadoActual == Constantes.PorValidar
                        ? null
                        : "Solo se pueden aceptar órdenes que están por validar.";

                case Constantes.Despachado:
                    return estadoActual == Constantes.Pendiente
                        ? null
                        : "Solo se pueden despachar órdenes pendientes.";

                case Constantes.Facturada:
                    return estadoActual == Constantes.Despachado
                        ? null
                        : "Solo se pueden facturar órdenes despachadas.";

                default:
                    return "El cambio de estado solicitado no está permitido.";
            }
        }
    }
}
