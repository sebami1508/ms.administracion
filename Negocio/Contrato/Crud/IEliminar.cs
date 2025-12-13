using Comun.Dto;

namespace Negocio.Contrato.Crud
{
    public interface IEliminar
    {
        Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param);
    }
}
