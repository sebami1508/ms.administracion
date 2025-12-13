using Comun.Dto;

namespace Negocio.Contrato.Crud
{
    public interface IConsulta
    {
        Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>();

    }
}
