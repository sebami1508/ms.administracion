using Comun.Dto.DtoParameter;
using Comun.Dto;
using Comun.Dto.DtoReader;
using Comun.Enumeracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;
using Web.Api.Extension;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdenController : ControllerBase
    {
        private readonly IOrden orden;
        private readonly ILogger<OrdenController> logger;

        public OrdenController(IOrden _orden, ILogger<OrdenController> _logger)
        {
            orden = _orden ?? throw new ArgumentException(nameof(OrdenController));
            logger = _logger;
        }

        /// <summary>
        /// El personal crea órdenes en cualquier estado inicial. Un cliente (App)
        /// solo puede crear órdenes propias y "Por validar".
        /// </summary>
        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(COrdenDto? _param)
        {
            if (_param != null && !User.EsPersonal())
            {
                var usuarioId = User.UsuarioId();
                if (string.IsNullOrWhiteSpace(usuarioId))
                    return this.NoAutorizado();

                _param.UsuarioId = usuarioId;
                _param.EstadoId = Constantes.PorValidar;
            }

            return Ok(await orden.GuardarAsync<COrdenDto, bool>(_param));
        }

        /// <summary>Solo el personal; el cambio de estado se valida contra el rol.</summary>
        [HttpPut]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesPersonal)]
        public async Task<IActionResult> Actualizar(UOrdenDto? _param)
        {
            var motivo = await orden.ValidarCambioEstadoAsync(_param?.OrdenId, _param?.EstadoId, User.RolesId());
            if (motivo != null)
            {
                logger.LogWarning("Cambio de estado rechazado. Orden {OrdenId}, estado {EstadoId}, usuario {UsuarioId}: {Motivo}",
                    _param?.OrdenId, _param?.EstadoId, User.UsuarioId(), motivo);
                return Ok(new RespuestaDto<string>(EstadoOperacion.Validacion, motivo));
            }

            return Ok(await orden.ActualizarAsync<UOrdenDto, string>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesPersonal)]
        public async Task<IActionResult> AceptarOrden([FromQuery] string _param)
        {
            return Ok(await orden.AceptarOrdenAsync<string, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await orden.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesPersonal)]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await orden.ConsultarListaAsync<List<ROrdenDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesPersonal)]
        public async Task<IActionResult> ConsultarListaPorEstadoId(string _param)
        {
            return Ok(await orden.ConsultarListaPorEstadoIdAsync<string, List<ROrdenDto>>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaOrdenesDelDia()
        {
            return Ok(SoloPropiasSiEsCliente(await orden.ConsultarListaOrdenesDelDiaAsync<List<ROrdenDto>>()));
        }

        [HttpPost]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesPersonal)]
        public async Task<IActionResult> ConsultarListaOrdenesRangoDeFechas(PFiltroOrdenesDto _dto)
        {
            return Ok(await orden.ConsultarListaOrdenesRangoDeFechasAsync<PFiltroOrdenesDto, List<ROrdenDto>>(_dto));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaOrdenesPorTurnoId(string _param)
        {
            return Ok(SoloPropiasSiEsCliente(await orden.ConsultarListaOrdenesPorTurnoIdAsync<string, List<ROrdenDto>>(_param)));
        }

        /// <summary>
        /// Un cliente solo recibe sus propias órdenes (antes la App las filtraba en
        /// el teléfono, pero el API entregaba las de todos, con nombres y direcciones).
        /// </summary>
        private RespuestaDto<List<ROrdenDto>> SoloPropiasSiEsCliente(RespuestaDto<List<ROrdenDto>> respuesta)
        {
            if (User.EsPersonal() || respuesta?.Respuesta == null)
                return respuesta!;

            var usuarioId = User.UsuarioId();
            respuesta.Respuesta = respuesta.Respuesta
                .Where(o => usuarioId != null && string.Equals(o.UsuarioId, usuarioId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return respuesta;
        }
    }
}
