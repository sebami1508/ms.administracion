using Comun.Dto.DtoParameter;
using Comun.Dto.DtoUtilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DistribuidorController : ControllerBase
    {
        #region Atributos
        private readonly IDistribuidor distribuidor;
        private readonly ILogger<DistribuidorController> logger;
        #endregion

        #region Constructor
        public DistribuidorController(IDistribuidor _distribuidor, ILogger<DistribuidorController> _logger)
        {
            distribuidor = _distribuidor ?? throw new ArgumentException(nameof(DistribuidorController));
            logger = _logger;
        }
        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CDistribuidorDto? _param)
        {
            return Ok(await distribuidor.GuardarAsync<CDistribuidorDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UDistribuidorDto? _param)
        {
            return Ok(await distribuidor.ActualizarAsync<UDistribuidorDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(EliminarDto _param)
        {
            return Ok(await distribuidor.EliminarAsync<EliminarDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await distribuidor.ConsultarListaAsync<List<RDistribuidorDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarPorNumeroIdentificacion(decimal _param)
        {
            return Ok(await distribuidor.ConsultarPorNumeroIdentificacionAsync<decimal, RDistribuidorDto>(_param));
        }
        #endregion
    }
}
