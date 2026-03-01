using Comun.Dto;
using Comun.Dto.DtoCreate;
using Comun.Dto.DtoReader;
using Comun.Dto.DtoUpdate;
using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;

namespace Negocio.Gestion
{
    public class ZonaGeograficaLogica : IZonaGeografica
    {
        private readonly ContextoDb db;
        private readonly CZonaGeograficaValidator validatorC;
        private readonly UZonaGeograficaValidator validatorU;
        public ZonaGeograficaLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new CZonaGeograficaValidator();
            validatorU = new UZonaGeograficaValidator();
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CZonaGeograficaDto;
            if (dto == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Parámetros inválidos.");

            var respuestaValidacion = await validatorC.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = new TaZonaGeograficaModel
            {
                ZonaGeograficaId = Guid.NewGuid().ToString(),
                Descripcion = dto.Descripcion!.Trim().ToUpper(),
                CodigoDane = (int?)dto.CodigoDane,
                Longitud = dto.Longitud,
                Latitud = dto.Latitud,
                PadreId = dto.PadreId,
                CodigoIso = dto.CodigoIso
            };

            await db.TaZonaGeograficaModel.AddAsync(model);
            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UZonaGeograficaDto;
            if (dto == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Parámetros inválidos.");

            var respuestaValidacion = await validatorU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaZonaGeograficaModel.FindAsync(dto.ZonaGeograficaId);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La zona geográfica no existe.");

            new GestionLogica(db).ActualizarCamposAutomatico(dto, model);

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as EliminarDto;
            if (dto == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Parámetros inválidos.");

            var model = await db.TaZonaGeograficaModel.FindAsync(dto.Id);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La zona geográfica no existe.");

            db.Entry(model).State = EntityState.Deleted;
            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaZonaGeograficaModel.Select(f => new RZonaGeograficaDto
            {
                ZonaGeograficaId = f.ZonaGeograficaId,
                Descripcion = f.Descripcion,
                CodigoDane = f.CodigoDane,
                Longitud = f.Longitud,
                Latitud = f.Latitud,
                PadreId = f.PadreId,
                CodigoIso = f.CodigoIso
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RZonaGeograficaDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaDepartamentosAsync<TReturn>()
        {
            var resultados = await db.TaZonaGeograficaModel.Where(x => x.PadreId == Constantes.GuidColombiaId).Select(f => new RZonaGeograficaDto
            {
                ZonaGeograficaId = f.ZonaGeograficaId,
                Descripcion = f.Descripcion,
                CodigoDane = f.CodigoDane,
                Longitud = f.Longitud,
                Latitud = f.Latitud,
                PadreId = f.PadreId,
                CodigoIso = f.CodigoIso
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RZonaGeograficaDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaMunicipiosPorDepartamentoIdAsync<TParam, TReturn>(TParam _param)
        {
            string departamentoId = _param as string;

            var resultados = await db.TaZonaGeograficaModel.Where(x => x.PadreId == departamentoId).Select(f => new RZonaGeograficaDto
            {
                ZonaGeograficaId = f.ZonaGeograficaId,
                Descripcion = f.Descripcion,
                CodigoDane = f.CodigoDane,
                Longitud = f.Longitud,
                Latitud = f.Latitud,
                PadreId = f.PadreId,
                CodigoIso = f.CodigoIso
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RZonaGeograficaDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }
    }
}
