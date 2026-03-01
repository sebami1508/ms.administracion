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
    public class RolLogica : IRol
    {
        #region Atributos
        private readonly ContextoDb db;
        private readonly CRolValidator validatorC;
        private readonly URolValidator validatorU;
        #endregion

        #region Constructores
        public RolLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new CRolValidator();
            validatorU = new URolValidator();
        }
        #endregion

        #region Métodos
        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CRolDto;
            var respuestaValidacion = await validatorC.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            // Duplicado por descripción
            var existente = await db.TaRolModel.FirstOrDefaultAsync(x => x.Descripcion == dto.Descripcion);
            if (existente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un rol con la misma descripción.");

            var model = new TaRolModel
            {
                RolId = Guid.NewGuid().ToString(),
                Descripcion = dto.Descripcion!.Trim().ToUpper(),
                Vigente = dto.Vigente ?? true
            };

            db.Add(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as URolDto;
            var respuestaValidacion = await validatorU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaRolModel.FindAsync(dto.RolId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El rol no existe.",
                };
            }

            // Actualizar campos manualmente
            model.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? model.Descripcion : dto.Descripcion.Trim().ToUpper();
            if (dto.Vigente.HasValue)
                model.Vigente = dto.Vigente.Value;

            db.Update(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var rolId = _param as string;
            if (string.IsNullOrWhiteSpace(rolId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.TaRolModel.FindAsync(rolId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El rol no existe.",
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
            var resultados = await db.TaRolModel.Select(f => new RRolDto
            {
                RolId = f.RolId,
                Descripcion = f.Descripcion,
                Vigente = f.Vigente
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RRolDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaVigentesAsync<TReturn>()
        {
            var resultados = await db.TaRolModel.Where(x => x.Vigente == true).Select(f => new RRolDto
            {
                RolId = f.RolId,
                Descripcion = f.Descripcion,
                Vigente = f.Vigente
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RRolDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as URolDto;
            if (dto?.RolId == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.TaRolModel.FindAsync(dto.RolId);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe el rol.");

            if (dto.Vigente.HasValue)
                model.Vigente = dto.Vigente.Value;
            else
                model.Vigente = !model.Vigente; // toggle

            db.Update(model);
            bool resultado = await db.SaveChangesAsync() > 0;
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }
        #endregion
    }
}
