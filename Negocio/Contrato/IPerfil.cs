using Comun.Dto;
using Negocio.Contrato.Crud;
using System.Collections.Generic;

namespace Negocio.Contrato
{
    public interface IPerfil : IGuardar, IConsulta, IActualizar, IEliminar
    {
        Task<RespuestaDto<TReturn>> ConsultarMenusPorRolesAsync<TParam, TReturn>(TParam _param);
        Task<RespuestaDto<TReturn>> ConsultarMenusPorRolAsync<TParam, TReturn>(TParam _param); // nuevo servicio
    }
}
