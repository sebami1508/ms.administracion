using Comun.Dto;
using Negocio.Contrato.Crud;
using System.IO;

namespace Negocio.Contrato
{
    public interface IProducto : IGuardar, IConsulta, IEliminar, IActualizar
    {
        Task<RespuestaDto<TReturn>> ExportarExcelAsync<TReturn>();
        Task<RespuestaDto<TReturn>> ImportarExcelAsync<TParam, TReturn>(TParam _param);
    }
}
