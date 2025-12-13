using Comun.Dto.DtoParameter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenuController : ControllerBase
    {
        #region Atributos
        private readonly IMenu menu;
        private readonly ILogger<MenuController> logger;
        #endregion

        #region Constructores
        public MenuController(IMenu _menu, ILogger<MenuController> _logger)
        {
            menu = _menu ?? throw new ArgumentException(nameof(MenuController));
            logger = _logger;
        }
        #endregion

        #region Métodos
        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await menu.ConsultarListaAsync<List<RMenuGrupoDto>>());
        }
        #endregion
    }
}
