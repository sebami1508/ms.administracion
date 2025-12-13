using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IRolUsuario : IGuardar, IConsulta, IEliminar
	{
        Task<RespuestaDto<TReturn>> ConsultarListaRolesAsync<TReturn>();
        Task<RespuestaDto<TReturn>> ConsultarListaRolesUsuarioIdAsync<TParam, TReturn>(TParam _param);
    }
}
