using Comun.Dto;

namespace Negocio.Contrato.Crud
{
    public interface IActualizar
    {
        Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param);

    }
}
