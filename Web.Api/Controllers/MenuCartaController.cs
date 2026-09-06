using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenuCartaController : ControllerBase
    {
        private readonly IMenuCarta menuCarta;
        private readonly ILogger<MenuCartaController> logger;

        public MenuCartaController(IMenuCarta _menuCarta, ILogger<MenuCartaController> _logger)
        {
            menuCarta = _menuCarta ?? throw new ArgumentException(nameof(MenuCartaController));
            logger = _logger;
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CMenuCartaDto? _param)
        {
            return Ok(await menuCarta.GuardarAsync<CMenuCartaDto, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarActual()
        {
            return Ok(await menuCarta.ConsultarActualAsync<Comun.Dto.RMenuCartaDto>());
        }
    }
}
