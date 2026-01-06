using Comun.Dto.DtoParameter;
using Comun.Dto;
using Comun.Dto.DtoReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemController : ControllerBase
    {
        private readonly IItem item;
        private readonly ILogger<ItemController> logger;

        public ItemController(IItem _item, ILogger<ItemController> _logger)
        {
            item = _item ?? throw new ArgumentException(nameof(ItemController));
            logger = _logger;
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CItemDto? _param)
        {
            return Ok(await item.GuardarAsync<CItemDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UItemDto? _param)
        {
            return Ok(await item.ActualizarAsync<UItemDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await item.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await item.ConsultarListaAsync<List<RItemDto>>());
        }
    }
}
