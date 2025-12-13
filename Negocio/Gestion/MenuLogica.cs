using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;

namespace Negocio.Gestion
{
    public class MenuLogica : IMenu
    {
        #region Atributos
        private readonly ContextoDb db;
        #endregion

        #region Constructores
        public MenuLogica(ContextoDb _db)
        {
            db = _db;
        }
        #endregion

        #region Métodos
        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            // Obtener todos los menús vigentes
            var menus = await db.TaMenuModel.Where(x => x.Vigente).ToListAsync();

            // Separar principales y submenús
            var principales = menus.Where(m => !m.SubMenu).OrderBy(m => m.Orden).ToList();
            var submenus = menus.Where(m => m.SubMenu).ToList();

            var grupos = new List<RMenuGrupoDto>();

            foreach (var principal in principales)
            {
                var grupo = new RMenuGrupoDto
                {
                    MenuId = principal.MenuId,
                    Nombre = principal.Nombre,
                    Icono = principal.Icono,
                    Ruta = principal.Ruta,
                    Orden = principal.Orden,
                    Vigente = principal.Vigente,
                    SubMenus = submenus.Where(s => s.MenuPadre == principal.MenuId)
                        .OrderBy(s => s.Orden)
                        .Select(f => new RMenuDto
                        {
                            MenuId = f.MenuId,
                            Nombre = f.Nombre,
                            Icono = f.Icono,
                            Ruta = f.Ruta,
                            SubMenu = f.MenuPadre,
                            MenuPadre = f.MenuPadre != null,
                            Orden = f.Orden,
                            Vigente = f.Vigente
                        }).ToList()
                };
                grupos.Add(grupo);
            }

            if (grupos.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(grupos, typeof(List<RMenuGrupoDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }
        #endregion
    }
}
