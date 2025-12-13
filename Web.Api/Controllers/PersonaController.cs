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
    public class PersonaController : ControllerBase
    {
        #region Atributos

        private readonly IPersona persona;
        private readonly ILogger<PersonaController> logger;

        #endregion

        #region Constructores

        public PersonaController(IPersona _persona, ILogger<PersonaController> _logger)
        {
            persona = _persona ?? throw new ArgumentException(nameof(PersonaController));
            logger = _logger;
        }

        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CPersonaDto? _param)
        {
            return Ok(await persona.GuardarAsync<CPersonaDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UPersonaDto? _param)
        {
            return Ok(await persona.ActualizarAsync<UPersonaDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(EliminarDto _param)
        {
            return Ok(await persona.EliminarAsync<EliminarDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await persona.ConsultarListaAsync<List<RPersonaDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarPorNumeroIdentificacion(decimal _param)
        {
            return Ok(await persona.ConsultarPorNumeroIdentificacionAsync<decimal, RPersonaDto>(_param));
        }

        #endregion
    }
}
