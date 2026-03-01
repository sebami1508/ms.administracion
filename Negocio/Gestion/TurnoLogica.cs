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
    public class TurnoLogica : ITurno
    {
        #region Atributos

        private readonly ContextoDb db;
        private readonly CTurnoValidator validacionC;
        private readonly UTurnoValidator validacionU;

        #endregion

        #region Constructores

        public TurnoLogica(ContextoDb _db)
        {
            db = _db;
            validacionC = new CTurnoValidator();
            validacionU = new UTurnoValidator();
        }

        #endregion

        #region Métodos

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CTurnoDto;
            var respuestaValidacion = await validacionC.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var turnoExiste = await db.TaTurnoModel.AsNoTracking().Where(x => x.FechaTurno.Date == dto.FechaTurno.Date).FirstOrDefaultAsync();

            if (turnoExiste != null)
                return new RespuestaDto<TReturn>(
                    EstadoOperacion.Malo,
                    "Ya existe un turno registrado para este día. Si fue finalizado por error, solicite a un administrador del sistema que lo habilite nuevamente."
                );

            var turnoPendiente = await db.TaTurnoModel.AsNoTracking().Where(x => x.EstadoId == Constantes.PendienteIniciar).FirstOrDefaultAsync();

            if (turnoPendiente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Existe un turno pendiente por iniciar, no se puede crear uno nuevo hasta finalizar el actual.");

            var turnoVigente = await db.TaTurnoModel.AsNoTracking().Where(x => x.EstadoId == Constantes.TurnoVigente).FirstOrDefaultAsync();

            if (turnoVigente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Existe un turno vigente, no se puede crear uno nuevo hasta finalizar el actual.");

            var nuevoTuurno = new TaTurnoModel
            {
                TurnoId = Guid.NewGuid().ToString(),
                UsuarioId = dto.UsuarioId,
                EstadoId = Constantes.PendienteIniciar,
                FechaTurno = DateTime.SpecifyKind(dto.FechaTurno, DateTimeKind.Unspecified),
                Base = dto.Base
            };

            db.TaTurnoModel.Add(nuevoTuurno);

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            if (_param is not UTurnoDto dto)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "Parámetro inválido. Se esperaba un UTurnoDto."
                };
            }

            var respuestaValidacion = await validacionU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            // Normalizar DateTime sin zona
            var fechaInicio = dto.FechaInicio.HasValue
                ? DateTime.SpecifyKind(dto.FechaInicio.Value, DateTimeKind.Unspecified)
                : (DateTime?)null;

            var fechaFin = dto.FechaFin.HasValue
                ? DateTime.SpecifyKind(dto.FechaFin.Value, DateTimeKind.Unspecified)
                : (DateTime?)null;

            var rows = await db.TaTurnoModel
                .Where(x => x.TurnoId == dto.TurnoId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EstadoId, x => dto.EstadoId ?? x.EstadoId)
                    .SetProperty(x => x.Base, x => dto.Base ?? x.Base)
                    .SetProperty(t => t.FechaInicio, t => dto.FechaInicio.HasValue ? fechaInicio : t.FechaInicio)
                    .SetProperty(t => t.FechaFin, t => dto.FechaFin.HasValue ? fechaFin : t.FechaFin)
                );

            if (rows == 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El turno no existe.");

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var id = _param as string;

            if (string.IsNullOrWhiteSpace(id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var model = await db.TaTurnoModel.FindAsync(id);

            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El turno no existe.",
                };
            }

            var ordenes = await db.TaOrdenModel.AsNoTracking().CountAsync(x => x.TurnoId == id);

            if(ordenes > 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se puede eliminar el turno, por que existen ordenes asociadas al mismo.");

            db.Remove(model);

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }


        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            DateTime fecha = DateTime.Now.AddMonths(-12);

            var resultados = await db.TaTurnoModel
                .OrderByDescending(c => c.FechaTurno)
                .Where(x => x.FechaTurno >= fecha)
                .Select(f => new RTurnoDto
                {
                    TurnoId = f.TurnoId,
                    UsuarioId = f.UsuarioId,
                    UsuarioStr = $"{f.TaUsuarioModel.Nombres} {f.TaUsuarioModel.Apellidos}",
                    EstadoId = f.EstadoId,
                    EstadoStr = f.TaDominioModel.Descripcion,
                    FechaTurno = f.FechaTurno,
                    FechaInicio = f.FechaInicio,
                    FechaFin = f.FechaFin,
                    Base = f.Base
                }).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RTurnoDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarTurnoVigenteAsync<TReturn>()
        {

            var resultados = await db.TaTurnoModel
               .Where(x => x.EstadoId == Constantes.TurnoVigente)
               .Select(f => new RTurnoDto
               {
                   TurnoId = f.TurnoId,
                   UsuarioId = f.UsuarioId,
                   UsuarioStr = $"{f.TaUsuarioModel.Nombres} {f.TaUsuarioModel.Apellidos}",
                   EstadoId = f.EstadoId,
                   EstadoStr = f.TaDominioModel.Descripcion,
                   FechaTurno = f.FechaTurno,
                   FechaInicio = f.FechaInicio,
                   FechaFin = f.FechaFin,
                   Base = f.Base
               }).FirstOrDefaultAsync();

            if (resultados != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(RTurnoDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        #endregion

    }
}
