using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Microsoft.AspNetCore.Authorization;
using Web.Api.Extension;
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
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> Guardar(CUsuarioDto? _param)
        {
            return Ok(await usuario.GuardarAsync<CUsuarioDto, bool>(_param));
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarCodigoRegistro(SolicitarOtpRegistroDto? _param)
        {
            return Ok(await usuario.SolicitarOtpRegistroAsync<SolicitarOtpRegistroDto, bool>(_param));
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Registrar(CRegistroClienteDto? _param)
        {
            return Ok(await usuario.RegistrarClienteAsync<CRegistroClienteDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UUsuarioDto? _param)
        {
            if (!User.EsAdministrador() && !User.EsElMismo(_param?.UsuarioId))
                return this.NoAutorizado("Solo puede actualizar sus propios datos.");

            return Ok(await usuario.ActualizarAsync<UUsuarioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> ActualizarVigencia(UUsuarioDto? _param)
        {
            return Ok(await usuario.ActualizarVigenciaAsync<UUsuarioDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> Eliminar(string? _param)
        {
            return Ok(await usuario.EliminarAsync<string, bool>(_param));
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<IActionResult> SolicitarCodigoReset(SolicitarOtpResetDto? _param)
        {
            return Ok(await usuario.SolicitarOtpResetPasswordAsync<SolicitarOtpResetDto, bool>(_param));
        }

        [HttpPost]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<IActionResult> RestablecerPassword(RestablecerPasswordOtpDto? _param)
        {
            return Ok(await usuario.RestablecerPasswordConOtpAsync<RestablecerPasswordOtpDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> ActualizarPassword(UUsuarioDto? _param)
        {
            if (!User.EsElMismo(_param?.UsuarioId))
                return this.NoAutorizado("Solo puede cambiar su propia contraseña.");

            return Ok(await usuario.ActualizarPasswordAsync<UUsuarioDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await usuario.ConsultarListaAsync<List<RUsuarioDto>>());
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize(Roles = Constantes.RolesAdministradores)]
        public async Task<IActionResult> EnviarNuevaPassword(string? _param)
        {
            return Ok(await usuario.EnviarNuevaPasswordAsync<string, bool>(_param));
        }


        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarUsuarioPorIdentificacion(string? _param)
        {
            if (!User.EsPersonal() && !User.EsMiIdentificacion(_param))
                return this.NoAutorizado("Solo puede consultar sus propios datos.");

            return Ok(await usuario.ConsultarUsuarioPorIdentificacionAnsync<string, RUsuarioDatosDto>(_param));
        }

        #endregion

    }
}
