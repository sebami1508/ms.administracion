using Comun.Dto;
using Comun.Enumeracion;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Api.Extension
{
    /// <summary>
    /// Identidad del usuario que hace la petición, tomada del JWT emitido por
    /// ms.seguridad (sub = UsuarioId, role = RolId, identificacion).
    /// </summary>
    public static class UsuarioActualExtension
    {
        public static string? UsuarioId(this ClaimsPrincipal usuario) =>
            usuario.FindFirstValue(ClaimTypes.NameIdentifier) ?? usuario.FindFirstValue("sub");

        public static string? Identificacion(this ClaimsPrincipal usuario) =>
            usuario.FindFirstValue("identificacion");

        public static IReadOnlyCollection<string> RolesId(this ClaimsPrincipal usuario) =>
            usuario.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        public static bool EsAdministrador(this ClaimsPrincipal usuario) =>
            usuario.IsInRole(Constantes.RolSuperAdministrador) || usuario.IsInRole(Constantes.RolAdministrador);

        /// <summary>Personal del negocio: administradores o ventas.</summary>
        public static bool EsPersonal(this ClaimsPrincipal usuario) =>
            usuario.EsAdministrador() || usuario.IsInRole(Constantes.RolVentas);

        /// <summary>true si el recurso pertenece a quien hace la petición.</summary>
        public static bool EsElMismo(this ClaimsPrincipal usuario, string? usuarioId) =>
            !string.IsNullOrWhiteSpace(usuarioId) &&
            string.Equals(usuario.UsuarioId(), usuarioId.Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>true si la identificación recibida es la de quien hace la petición.</summary>
        public static bool EsMiIdentificacion(this ClaimsPrincipal usuario, string? identificacion) =>
            decimal.TryParse(identificacion?.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var pedida) &&
            decimal.TryParse(usuario.Identificacion(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var propia) &&
            pedida == propia;

        /// <summary>403 con el formato de respuesta estándar del API.</summary>
        public static ObjectResult NoAutorizado(this ControllerBase controlador, string mensaje = "No tiene permisos para realizar esta operación.") =>
            controlador.StatusCode(StatusCodes.Status403Forbidden, new RespuestaDto<object>(EstadoOperacion.Malo, mensaje));
    }
}
