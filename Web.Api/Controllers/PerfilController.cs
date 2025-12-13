using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        #region Atributos
        private readonly IPerfil perfil;
        private readonly ILogger<PerfilController> logger;
        #endregion

        #region Constructores
        public PerfilController(IPerfil _perfil, ILogger<PerfilController> _logger)
        {
            perfil = _perfil ?? throw new ArgumentException(nameof(PerfilController));
            logger = _logger;
        }
        #endregion

        #region Métodos
        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CPerfilDto? _param)
        {
            return Ok(await perfil.GuardarAsync<CPerfilDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UPerfilDto? _param)
        {
            return Ok(await perfil.ActualizarAsync<UPerfilDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await perfil.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await perfil.ConsultarListaAsync<List<RPerfilDto>>());
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarMenusPorRoles(PRolesUsuarioDto _dto)
        {
            return Ok(await perfil.ConsultarMenusPorRolesAsync<PRolesUsuarioDto, List<RMenuGrupoDto>>(_dto));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarMenusPorRol(string _param)
        {
            return Ok(await perfil.ConsultarMenusPorRolAsync<string, List<RMenuGrupoDto>>(_param));
        }
        #endregion
    }
}
