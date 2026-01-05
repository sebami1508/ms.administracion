using Comun.Dto.DtoParameter;
using Comun.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Negocio.Contrato;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IProducto producto;
        private readonly ILogger<ProductoController> logger;

        public ProductoController(IProducto _producto, ILogger<ProductoController> _logger)
        {
            producto = _producto ?? throw new ArgumentException(nameof(ProductoController));
            logger = _logger;
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> Guardar(CProductoDto? _param)
        {
            return Ok(await producto.GuardarAsync<CProductoDto, bool>(_param));
        }

        [HttpPut]
        [Route("[Action]")]
        public async Task<IActionResult> Actualizar(UProductoDto? _param)
        {
            return Ok(await producto.ActualizarAsync<UProductoDto, bool>(_param));
        }

        [HttpDelete]
        [Route("[Action]")]
        public async Task<IActionResult> Eliminar(string _param)
        {
            return Ok(await producto.EliminarAsync<string, bool>(_param));
        }

        [HttpGet]
        [Route("[Action]")]
        public async Task<IActionResult> ConsultarLista()
        {
            return Ok(await producto.ConsultarListaAsync<List<RProductoDto>>());
        }
    }
}
