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
    public class RolUsuarioController : ControllerBase
    {
        #region Atributos

        private readonly IRolUsuario rolUsuario;
        private readonly ILogger<RolUsuarioController> logger;

        #endregion

        #region Constructores

        public RolUsuarioController(IRolUsuario _rolesUsuario, ILogger<RolUsuarioController> _logger)
        {
            rolUsuario = _rolesUsuario ?? throw new ArgumentException(nameof(RolUsuarioController));
            logger = _logger;
        }

        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CRolUsuarioDto? _param)
        {
			return Ok(await rolUsuario.GuardarAsync<CRolUsuarioDto, bool>(_param));
		}

		[HttpDelete]
		[Route("[Action]")]
		public async Task<IActionResult> Eliminar(EliminarDto _dto)
		{
			return Ok(await rolUsuario.EliminarAsync<EliminarDto, bool>(_dto));
		}

		[HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await rolUsuario.ConsultarListaAsync<List<RRolUsuarioDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaRoles()
        {
            return Ok(await rolUsuario.ConsultarListaRolesAsync<List<RRolDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarListaRolesUsuarioId(string _param)
        {
            return Ok(await rolUsuario.ConsultarListaRolesUsuarioIdAsync<string, List<RRolUsuarioDto>>(_param));
        }

        #endregion

    }
}
