using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IOrden : IGuardar, IConsulta, IEliminar, IActualizar
    {
        Task<RespuestaDto<TReturn>> ConsultarListaOrdenesDelDiaAsync<TReturn>();
        Task<RespuestaDto<TReturn>> ConsultarListaOrdenesPorTurnoIdAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaOrdenesRangoDeFechasAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaPorEstadoIdAsync<TParam, TReturn>(TParam _param);
    }
}
