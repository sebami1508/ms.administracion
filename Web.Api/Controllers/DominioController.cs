using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DominioController : ControllerBase
    {
        #region Atributos

        private readonly IDominio dominio;
        private readonly ILogger<DominioController> logger;

        #endregion

        #region Constructores

        public DominioController(IDominio _dominio, ILogger<DominioController> _logger)
        {
            dominio = _dominio ?? throw new ArgumentException(nameof(DominioController));
            logger = _logger;
        }

        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CDominioDto? _param)
        {
            return Ok(await dominio.GuardarAsync<CDominioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UDominioDto? _param)
        {
            return Ok(await dominio.ActualizarAsync<UDominioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await dominio.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await dominio.ConsultarListaAsync<List<RDominioDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaPadres()
        {
            return Ok(await dominio.ConsultarListaPadresAsync<List<RDominioDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaHijosDependenPadreId(string _param)
        {
            return Ok(await dominio.ConsultarListaHijosDependenPadreIdAsync<string, List<RDominioDto>>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaHijosVigentesDependenPadreId(string _param)
        {
            return Ok(await dominio.ConsultarListaHijosVigentesDependenPadreIdAsync<string, List<RDominioDto>>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarPorDominioId(string _param)
        {
            return Ok(await dominio.ConsultarPorDominioIdAsync<string, RDominioDto>(_param));
        }

        #endregion
    }
}
