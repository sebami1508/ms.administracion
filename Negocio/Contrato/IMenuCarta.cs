using Comun.Dto;

namespace Negocio.Contrato
{
    public interface IMenuCarta
    {
        Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarActualAsync<TReturn>();
    }
}
