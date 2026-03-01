using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;

namespace Negocio.Gestion
{
    public class PerfilLogica : IPerfil
    {
        #region Atributos
        private readonly ContextoDb db;
        private readonly CPerfilValidator validatorC;
        private readonly UPerfilValidator validatorU;
        #endregion

        #region Constructores
        public PerfilLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new CPerfilValidator();
            validatorU = new UPerfilValidator();
        }
        #endregion

        #region Métodos
        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CPerfilDto;
            var respuestaValidacion = await validatorC.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            // Duplicado por Menu+Rol
            var existente = await db.TaPerfilModel.FirstOrDefaultAsync(x => x.MenuId == dto.MenuId && x.RolId == dto.RolId);
            if (existente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un perfil para el menú y rol seleccionados.");

            var model = new TaPerfilModel
            {
                PerfilId = Guid.NewGuid().ToString(),
                MenuId = dto.MenuId!,
                RolId = dto.RolId!
            };

            db.Add(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UPerfilDto;
            var respuestaValidacion = await validatorU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaPerfilModel.FindAsync(dto.PerfilId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El perfil no existe.",
                };
            }

            // Validar duplicado si cambia
            if ((model.MenuId != dto.MenuId || model.RolId != dto.RolId) &&
                await db.TaPerfilModel.AnyAsync(x => x.MenuId == dto.MenuId && x.RolId == dto.RolId && x.PerfilId != dto.PerfilId))
            {
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe otro perfil con el menú y rol seleccionados.");
            }

            model.MenuId = dto.MenuId!;
            model.RolId = dto.RolId!;

            db.Update(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var perfilId = _param as string;
            if (string.IsNullOrWhiteSpace(perfilId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.TaPerfilModel.FindAsync(perfilId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El perfil no existe.",
                };
            }

            db.Remove(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaPerfilModel.Select(f => new RPerfilDto
            {
                PerfilId = f.PerfilId,
                MenuId = f.MenuId,
                RolId = f.RolId,
                NombreMenu = db.TaMenuModel.Where(m => m.MenuId == f.MenuId).Select(m => m.Nombre).FirstOrDefault(),
                DescripcionRol = db.TaRolModel.Where(r => r.RolId == f.RolId).Select(r => r.Descripcion).FirstOrDefault()
            }).OrderBy(o => o.NombreMenu).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RPerfilDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarMenusPorRolesAsync<TParam, TReturn>(TParam _param)
        {
            var roles = _param as PRolesUsuarioDto;
            if (roles == null || roles.Roles.Count == 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Lista de roles vacía.");

            // Obtener perfiles que estén asociados a los roles suministrados
            var perfiles = await db.TaPerfilModel
                .Where(p => roles.Roles.Contains(p.RolId))
                .Select(p => p.MenuId)
                .Distinct()
                .ToListAsync();

            if (perfiles.Count == 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No hay menús asociados a los roles proporcionados.");

            // Obtener menús vigentes filtrados por perfiles
            var menus = await db.TaMenuModel
                .Where(m => m.Vigente && perfiles.Contains(m.MenuId))
                .ToListAsync();

            var principales = menus.Where(m => !m.SubMenu).OrderBy(m => m.Orden).ToList();
            var submenus = menus.Where(m => m.SubMenu).ToList();

            var grupos = new List<RMenuGrupoDto>();

            foreach (var principal in principales)
            {
                var grupo = new RMenuGrupoDto
                {
                    MenuId = principal.MenuId,
                    PerfilId = db.TaPerfilModel.Where(pp => pp.MenuId == principal.MenuId && roles.Roles.Contains(pp.RolId)).Select(pp => pp.PerfilId).FirstOrDefault(),
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
                            PerfilId = db.TaPerfilModel.Where(pp => pp.MenuId == f.MenuId && roles.Roles.Contains(pp.RolId)).Select(pp => pp.PerfilId).FirstOrDefault(),
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

        public async Task<RespuestaDto<TReturn>> ConsultarMenusPorRolAsync<TParam, TReturn>(TParam _param)
        {
            var rolId = _param as string;
            if (string.IsNullOrWhiteSpace(rolId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "RolId inválido.");

            var menuIds = await db.TaPerfilModel
                .Where(p => p.RolId == rolId)
                .Select(p => p.MenuId)
                .Distinct()
                .ToListAsync();

            if (menuIds.Count == 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "El rol no tiene menús asociados.");

            var menus = await db.TaMenuModel
                .Where(m => m.Vigente && menuIds.Contains(m.MenuId))
                .ToListAsync();

            var principales = menus.Where(m => !m.SubMenu).OrderBy(m => m.Orden).ToList();
            var submenus = menus.Where(m => m.SubMenu).ToList();

            var grupos = new List<RMenuGrupoDto>();

            foreach (var principal in principales)
            {
                var grupo = new RMenuGrupoDto
                {
                    MenuId = principal.MenuId,
                    PerfilId = db.TaPerfilModel.Where(pp => pp.MenuId == principal.MenuId && pp.RolId == rolId).Select(pp => pp.PerfilId).FirstOrDefault(),
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
                            PerfilId = db.TaPerfilModel.Where(pp => pp.MenuId == f.MenuId && pp.RolId == rolId).Select(pp => pp.PerfilId).FirstOrDefault(),
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
