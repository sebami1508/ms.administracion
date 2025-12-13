using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolController : ControllerBase
    {
        #region Atributos
        private readonly IRol rol;
        private readonly ILogger<RolController> logger;
        #endregion

        #region Constructores
        public RolController(IRol _rol, ILogger<RolController> _logger)
        {
            rol = _rol ?? throw new ArgumentException(nameof(RolController));
            logger = _logger;
        }
        #endregion

        #region Métodos
        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CRolDto? _param)
        {
            return Ok(await rol.GuardarAsync<CRolDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(URolDto? _param)
        {
            return Ok(await rol.ActualizarAsync<URolDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await rol.EliminarAsync<string, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> ActualizarVigencia(URolDto? _param)
        {
            return Ok(await rol.ActualizarVigenciaAsync<URolDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await rol.ConsultarListaAsync<List<RRolDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaVigentes()
        {
            return Ok(await rol.ConsultarListaVigentesAsync<List<RRolDto>>());
        }
        #endregion
    }
}
