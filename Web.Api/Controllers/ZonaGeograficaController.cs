using Comun.Dto.DtoCreate;
using Comun.Dto.DtoReader;
using Comun.Dto.DtoUpdate;
using Comun.Dto.DtoUtilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ZonaGeograficaController : ControllerBase
    {
        private readonly IZonaGeografica zonaGeografica;
        private readonly ILogger<ZonaGeograficaController> logger;

        public ZonaGeograficaController(IZonaGeografica _zonaGeografica, ILogger<ZonaGeograficaController> _logger)
        {
            zonaGeografica = _zonaGeografica ?? throw new ArgumentException(nameof(ZonaGeograficaController));
            logger = _logger;
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar([FromBody] CZonaGeograficaDto _param)
        {
            return Ok(await zonaGeografica.GuardarAsync<CZonaGeograficaDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar([FromBody] UZonaGeograficaDto _param)
        {
            return Ok(await zonaGeografica.ActualizarAsync<UZonaGeograficaDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar([FromBody] EliminarDto _param)
        {
            return Ok(await zonaGeografica.EliminarAsync<EliminarDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await zonaGeografica.ConsultarListaAsync<List<RZonaGeograficaDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaDepartamentos()
        {
            return Ok(await zonaGeografica.ConsultarListaDepartamentosAsync<List<RZonaGeograficaDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaMunicipiosPorDepartamentoId(string _param)
        {
            return Ok(await zonaGeografica.ConsultarListaMunicipiosPorDepartamentoIdAsync<string, List<RZonaGeograficaDto>>(_param));
        }
    }
}
