using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IUsuario : IGuardar, IConsulta, IActualizar, IEliminar
    {
        Task<RespuestaDto<TReturn>> ActualizarPasswordAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarUsuarioPorIdentificacionAnsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> EnviarNuevaPasswordAsync<TParam, TReturn>(TParam _param);
    }
}
