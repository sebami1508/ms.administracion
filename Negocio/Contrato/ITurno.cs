using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface ITurno : IGuardar, IConsulta, IActualizar, IEliminar
    {
        Task<RespuestaDto<TReturn>> ConsultarTurnoVigenteAsync<TReturn>();
    }
}
