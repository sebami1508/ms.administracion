using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        #region Atributos

        private readonly IUsuario usuario;
        private readonly ILogger<UsuarioController> logger;

        #endregion

        #region Constructores

        public UsuarioController(IUsuario _usuario, ILogger<UsuarioController> _logger)
        {
            usuario = _usuario ?? throw new ArgumentException(nameof(UsuarioController));
            logger = _logger;
        }

        #endregion

        #region Métodos

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CUsuarioDto? _param)
        {
            return Ok(await usuario.GuardarAsync<CUsuarioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UUsuarioDto? _param)
        {
            return Ok(await usuario.ActualizarAsync<UUsuarioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> ActualizarVigencia(UUsuarioDto? _param)
        {
            return Ok(await usuario.ActualizarVigenciaAsync<UUsuarioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string? _param)
        {
            return Ok(await usuario.EliminarAsync<string, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> ActualizarPassword(UUsuarioDto? _param)
        {
            return Ok(await usuario.ActualizarPasswordAsync<UUsuarioDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await usuario.ConsultarListaAsync<List<RUsuarioDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> EnviarNuevaPassword(string? _param)
        {
            return Ok(await usuario.EnviarNuevaPasswordAsync<string, bool>(_param));
        }


        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarUsuarioPorIdentificacion(string? _param)
        {
            return Ok(await usuario.ConsultarUsuarioPorIdentificacionAnsync<string, RUsuarioDatosDto>(_param));
        }

        #endregion

    }
}
