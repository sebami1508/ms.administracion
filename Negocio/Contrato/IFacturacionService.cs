using Comun.Dto;
using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IFacturacionService
    {
        Task<string> GenerarNumeroFacturaAsync(string prefijo = "BRA-");
    }
}
