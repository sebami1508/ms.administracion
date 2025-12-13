using Comun.Dto;

namespace Negocio.Contrato.Crud
{
    public interface IGuardar
    {
        Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param);

    }
}
