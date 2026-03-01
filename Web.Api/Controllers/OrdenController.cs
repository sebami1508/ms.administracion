using Comun.Dto.DtoParameter;
using Comun.Dto;
using Comun.Dto.DtoReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

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

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(COrdenDto? _param)
        {
            return Ok(await orden.GuardarAsync<COrdenDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UOrdenDto? _param)
        {
            return Ok(await orden.ActualizarAsync<UOrdenDto, string>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await orden.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await orden.ConsultarListaAsync<List<ROrdenDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaPorEstadoId(string _param)
        {
            return Ok(await orden.ConsultarListaPorEstadoIdAsync<string, List<ROrdenDto>>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaOrdenesDelDia()
        {
            return Ok(await orden.ConsultarListaOrdenesDelDiaAsync<List<ROrdenDto>>());
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaOrdenesRangoDeFechas(PFiltroOrdenesDto _dto)
        {
            return Ok(await orden.ConsultarListaOrdenesRangoDeFechasAsync<PFiltroOrdenesDto, List<ROrdenDto>>(_dto));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaOrdenesPorTurnoId(string _param)
        {
            return Ok(await orden.ConsultarListaOrdenesPorTurnoIdAsync<string, List<ROrdenDto>>(_param));
        }
    }
}
