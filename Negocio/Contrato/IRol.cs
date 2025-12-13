using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IRol : IGuardar, IConsulta, IEliminar, IActualizar
    {
        Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaVigentesAsync<TReturn>();
    }
}
