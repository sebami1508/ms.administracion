using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DispositivoController : ControllerBase
    {
        private readonly IFcmService _fcm;

        public DispositivoController(IFcmService fcm)
        {
            _fcm = fcm ?? throw new ArgumentException(nameof(DispositivoController));
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> RegistrarToken(RegistrarTokenFcmDto? _param)
        {
            if (_param == null || string.IsNullOrWhiteSpace(_param.UsuarioId) ||
                string.IsNullOrWhiteSpace(_param.Token))
                return Ok(new RespuestaDto<bool>(EstadoOperacion.Validacion,
                    "Debe enviar el usuario y el token."));

            await _fcm.RegistrarTokenAsync(_param.UsuarioId, _param.Token, _param.Plataforma);
            return Ok(new RespuestaDto<bool>(EstadoOperacion.Bueno,
                "Token registrado correctamente.", true));
        }
    }
}
