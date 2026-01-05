using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IDominio : IGuardar, IConsulta, IActualizar, IEliminar
    {
        Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaHijosDependenPadreIdAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaHijosVigentesDependenPadreIdAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarListaPadresAsync<TReturn>();
        Task<RespuestaDto<TReturn>> ConsultarPorDominioIdAsync<TParam, TReturn>(TParam _param);
    }
}
