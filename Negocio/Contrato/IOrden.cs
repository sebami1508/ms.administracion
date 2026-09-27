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
        Task<RespuestaDto<TReturn>> AceptarOrdenAsync<TParam, TReturn>(TParam _param);

        /// <summary>
        /// Null si el usuario (según sus roles) puede aplicar ese cambio de estado a
        /// la orden; si no, el mensaje del motivo. Usa el estado guardado en la base.
        /// </summary>
        Task<string?> ValidarCambioEstadoAsync(string? ordenId, string? estadoNuevo, IReadOnlyCollection<string> roles);
    }
}
