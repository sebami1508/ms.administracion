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
    public class DominioLogica : IDominio
    {
        #region Atributos

        private readonly ContextoDb db;
        private readonly CDominioValidator validacionC;
        private readonly UDominioValidator validacionU;

        #endregion

        #region Constructores

        public DominioLogica(ContextoDb _db)
        {
            db = _db;
            validacionC = new CDominioValidator();
            validacionU = new UDominioValidator();
        }

        #endregion

        #region Métodos

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CDominioDto;
            var respuestaValidacion = await validacionC.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            if (dto.PadreId == null)
            {
                var dominios = await db.TaDominioModel.Where(x => x.PadreId == null).ToListAsync();

                if (dominios.Any(d => d.Descripcion == dto.Descripcion))
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un dominio padre con la misma descripción.");
            }

            if (dto.PadreId != null)
            {
                var dominiosDependenPadre = await db.TaDominioModel.Where(x => x.PadreId == dto.PadreId).ToListAsync();

                if (dominiosDependenPadre.Any(d => d.Descripcion == dto.Descripcion))
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un dominio con la misma descripción.");
            }

            // Consultar todos los valores de DominioId
            var dominioIds = await db.TaDominioModel.Select(d => d.DominioId).ToListAsync();

            db.Entry(new TaDominioModel
            {
                DominioId = Guid.NewGuid().ToString(),
                Descripcion = dto.Descripcion,
                PadreId = dto.PadreId,
                Vigente = true,
            }).State = EntityState.Added;

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UDominioDto;
            var validatorU = new UDominioValidator();
            var respuestaValidacion = await validacionU.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaDominioModel.FindAsync(dto.DominioId);

            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El dominio no existe.",
                };
            }

            GestionAuditoriaLogica auditoriaLogica = new GestionAuditoriaLogica(db);
            auditoriaLogica.ActualizarCamposAutomatico(dto, model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

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

            var dato = await db.TaDominioModel.FindAsync(dto.Id);

            if (dato == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "La no existe.",
                };
            }

            dato.Vigente = (dato.Vigente == Constantes.Vigente) ? Constantes.NoVigente : Constantes.Vigente;
            db.Update(dato);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto, true);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UDominioDto;

            var dato = await db.TaDominioModel.FindAsync(dto.DominioId);

            if (dato == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "La no existe.",
                };
            }

            dato.Vigente = dto.Vigente;
            db.Update(dato);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaDominioModel.OrderBy(c => c.PadreId)
                .ThenBy(c => c.DominioId)
                .Select(f => new RDominioDto
                {
                    DominioId = f.DominioId,
                    Descripcion = f.Descripcion,
                    PadreId = f.PadreId,
                    PadreIdStr = f.Padre.Descripcion,
                    Vigente = f.Vigente
                }).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RDominioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaPadresAsync<TReturn>()
        {
            var resultados = await db.TaDominioModel.Where(x => x.PadreId == null).Select(f => new RDominioDto
            {
                DominioId = f.DominioId,
                Descripcion = f.Descripcion,
                PadreId = f.PadreId,
                PadreIdStr = f.Padre.Descripcion,
                Vigente = f.Vigente
            }).OrderBy(c => c.Descripcion).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RDominioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaHijosDependenPadreIdAsync<TParam, TReturn>(TParam _param)
        {
            string padreId = _param as string;

            if (string.IsNullOrWhiteSpace(padreId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var resultados = await db.TaDominioModel.Where(x => x.PadreId == padreId).Select(f => new RDominioDto
            {
                DominioId = f.DominioId,
                Descripcion = f.Descripcion,
                PadreId = f.PadreId,
                PadreIdStr = f.Padre.Descripcion,
                Vigente = f.Vigente
            }).OrderBy(c => c.Descripcion).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RDominioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaHijosVigentesDependenPadreIdAsync<TParam, TReturn>(TParam _param)
        {
            string padreId = _param as string;

            if (string.IsNullOrWhiteSpace(padreId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var resultados = await db.TaDominioModel.Where(x => x.PadreId == padreId && x.Vigente == true).Select(f => new RDominioDto
            {
                DominioId = f.DominioId,
                Descripcion = f.Descripcion,
                PadreId = f.PadreId,
                PadreIdStr = f.Padre.Descripcion,
                Vigente = f.Vigente
            }).OrderBy(c => c.Descripcion).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RDominioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarPorDominioIdAsync<TParam, TReturn>(TParam _param)
        {
            string dominioId = _param as string;

            if (string.IsNullOrWhiteSpace(dominioId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var resultados = await db.TaDominioModel.Where(x => x.DominioId == dominioId).Select(f => new RDominioDto
            {
                DominioId = f.DominioId,
                Descripcion = f.Descripcion,
                PadreId = f.PadreId,
                PadreIdStr = f.Padre.Descripcion,
                Vigente = f.Vigente
            }).FirstOrDefaultAsync();

            if (resultados != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(RDominioDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        #endregion

    }
}
