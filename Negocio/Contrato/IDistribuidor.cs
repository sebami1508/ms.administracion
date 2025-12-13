using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IDistribuidor : IGuardar, IConsulta, IActualizar, IEliminar
    {
        Task<RespuestaDto<TReturn>> ConsultarPorNumeroIdentificacionAsync<TParam, TReturn>(TParam _param);
    }
}
