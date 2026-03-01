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
    public class TurnoController : ControllerBase
    {
        #region Atributos

        private readonly ITurno turno;
        private readonly ILogger<TurnoController> logger;

        #endregion

        #region Constructores

        public TurnoController(ITurno _turno, ILogger<TurnoController> _logger)
        {
            turno = _turno ?? throw new ArgumentException(nameof(TurnoController));
            logger = _logger;
        }

        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CTurnoDto? _param)
        {
            return Ok(await turno.GuardarAsync<CTurnoDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UTurnoDto? _param)
        {
            return Ok(await turno.ActualizarAsync<UTurnoDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await turno.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await turno.ConsultarListaAsync<List<RTurnoDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarTurnoVigente()
        {
            return Ok(await turno.ConsultarTurnoVigenteAsync<RTurnoDto>());
        }

        #endregion
    }
}
