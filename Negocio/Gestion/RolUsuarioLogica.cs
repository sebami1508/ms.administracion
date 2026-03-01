using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;
using FluentValidation;

namespace Negocio.Gestion
{
    public class RolUsuarioLogica : IRolUsuario
    {
        #region Atributos

        private readonly ContextoDb db;
        private readonly CRolUsuarioValidator validacionC;
        private readonly URolUsuarioValidator validacionU;

        #endregion

        #region Constructores

        public RolUsuarioLogica(ContextoDb _db)
        {
            db = _db;
            validacionC = new CRolUsuarioValidator();
            validacionU = new URolUsuarioValidator();
        }

        #endregion

        #region Métodos

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CRolUsuarioDto;
            var respuestaValidacion = await validacionC.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var rolesUsuario = await db.TaRolUsuarioModel.Where(x => x.UsuarioId == dto.UsuarioId).ToListAsync();

            if (rolesUsuario.Count > 0)
                foreach (var role in rolesUsuario)
                {
                    if (role.RolId == dto.RolId)
                        return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"El usuario ya tiene el rol asignado.");
                }

            var model = new TaRolUsuarioModel
            {
                RolUsuarioId = Guid.NewGuid().ToString(),
                RolId = dto.RolId,
                UsuarioId = dto.UsuarioId
            };

            db.Add(model);

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as EliminarDto;

            if (string.IsNullOrWhiteSpace(dto.Id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var dato = await db.TaRolUsuarioModel.FindAsync(dto.Id);

            if (dato == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El rol no existe.",
                };
            }

            db.Remove(dato);

            bool resultado = await db.SaveChangesAsync(true) > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaRolUsuarioModel.Select(f => new RRolUsuarioDto
            {
                RolUsuarioId = f.RolUsuarioId,
                RolId = f.RolId,
                UsuarioId = f.UsuarioId,
                DescripcionRol = f.TaRolModel.Descripcion
            }).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RRolUsuarioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaRolesAsync<TReturn>()
        {
            var resultados = await db.TaRolModel.Select(f => new RRolDto
            {
                RolId = f.RolId,
                Descripcion = f.Descripcion
            }).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RRolDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaRolesUsuarioIdAsync<TParam, TReturn>(TParam _param)
        {
            string usuarioId = _param as string;

            if (string.IsNullOrWhiteSpace(usuarioId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var resultados = await db.TaRolUsuarioModel.Where(x => x.UsuarioId == usuarioId).Select(f => new RRolUsuarioDto
            {
                RolUsuarioId = f.RolUsuarioId,
                UsuarioId = f.UsuarioId,
                RolId = f.RolId,
                DescripcionRol = f.TaRolModel.Descripcion
            }).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RRolUsuarioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        #endregion

    }
}
