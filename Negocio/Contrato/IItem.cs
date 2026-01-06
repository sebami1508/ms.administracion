using Negocio.Contrato.Crud;

namespace Negocio.Contrato
{
    public interface IItem : IGuardar, IConsulta, IEliminar, IActualizar
    {
    }
}
