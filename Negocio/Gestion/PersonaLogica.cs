using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;
using FluentValidation;
using Comun.Dto.DtoUtilidades;

namespace Negocio.Gestion
{
    public class PersonaLogica : IPersona
    {
        #region Atributos
        private readonly ContextoDb db;
        private readonly CPersonaValidator validatorC;
        private readonly UPersonaValidator validatorU;
        private readonly MyConfig _myConfig;
        #endregion

        #region Constructores
        public PersonaLogica(ContextoDb _db, MyConfig myConfig)
        {
            db = _db;
            validatorC = new CPersonaValidator();
            validatorU = new UPersonaValidator();
            _myConfig = myConfig;
        }
        #endregion

        #region Métodos
        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CPersonaDto;
            var respuestaValidacion = await validatorC.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            // Verificación duplicados por documento
            var existente = await db.TaPersonaModel.FirstOrDefaultAsync(x => x.TipoDocumentoId == dto.TipoDocumentoId && x.NumeroIdentificacion == dto.NumeroIdentificacion);
            if (existente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe una persona con el mismo tipo y número de documento.");

            // Verificación duplicados por correo
            var existenteCorreo = await db.TaPersonaModel.FirstOrDefaultAsync(x => x.Correo == dto.Correo);
            if (existente != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe una persona con el mismo correo electrónico.");

            // Generar password temporal
            string password = UtilidadesLogica.GenerarPassword();
            string passwordEncriptada = UtilidadesLogica.EncryptPassword(password, _myConfig.Key);

            var model = new TaPersonaModel
            {
                PersonaId = Guid.NewGuid().ToString(),
                Nombres = dto.Nombres!.Trim().ToUpper(),
                Apellidos = dto.Apellidos!.Trim().ToUpper(),
                TipoDocumentoId = dto.TipoDocumentoId!,
                NumeroIdentificacion = dto.NumeroIdentificacion!.Value,
                FechaExpedicion = dto.FechaExpedicion!.Value,
                Correo = dto.Correo!.Trim().ToLower(),
                Telefono = dto.Telefono!.Trim(),
                Direccion = dto.Direccion!.Trim(),
                FechaNacimiento = dto.FechaNacimiento!.Value,
                GeneroId = dto.GeneroId!,
                TerminosCondiciones = dto.TerminosCondiciones,
                PoliticaTratamientoDatos = dto.PoliticaTratamientoDatos,
                MayorEdad = dto.MayorEdad,
                ResponsabilidadFiscalId = dto.ResponsabilidadFiscalId!
            };

            db.Add(model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UPersonaDto;
            var respuestaValidacion = await validatorU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaPersonaModel.FindAsync(dto.PersonaId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "La persona no existe.",
                };
            }

            GestionAuditoriaLogica auditoria = new GestionAuditoriaLogica(db);
            auditoria.ActualizarCamposAutomatico(dto, model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);
            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as EliminarDto;
            var model = await db.TaPersonaModel.FindAsync(dto.Id);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "La persona no existe.",
                };
            }

            db.Entry(model).State = EntityState.Deleted;
            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto, true);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaPersonaModel.Select(f => new RPersonaDto
            {
                PersonaId = f.PersonaId,
                Nombres = f.Nombres,
                Apellidos = f.Apellidos,
                TipoDocumentoId = f.TipoDocumentoId,
                TipoDocumentoIdStr = f.TaDominioModelTipoDocumento!.Descripcion,
                NumeroIdentificacion = f.NumeroIdentificacion,
                FechaExpedicion = f.FechaExpedicion,
                Correo = f.Correo,
                Telefono = f.Telefono,
                Direccion = f.Direccion,
                FechaNacimiento = f.FechaNacimiento,
                GeneroId = f.GeneroId,
                GeneroIdStr = f.TaDominioModelTipoDocumento!.Descripcion,
                TerminosCondiciones = f.TerminosCondiciones,
                PoliticaTratamientoDatos = f.PoliticaTratamientoDatos,
                MayorEdad = f.MayorEdad,
                ResponsabilidadFiscalId = f.ResponsabilidadFiscalId,
                ResponsabilidadFiscalIdStr = f.TaDominioModelResponsabilidad!.Descripcion
            }).OrderBy(o => o.Nombres).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RPersonaDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarPorNumeroIdentificacionAsync<TParam, TReturn>(TParam _param)
        {
            var numero = _param as decimal?;
            if (numero == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificación inválida.");

            var resultado = await db.TaPersonaModel.Where(x => x.NumeroIdentificacion == numero.Value)
                .Select(f => new RPersonaDto
                {
                    PersonaId = f.PersonaId,
                    Nombres = f.Nombres,
                    Apellidos = f.Apellidos,
                    TipoDocumentoId = f.TipoDocumentoId,
                    TipoDocumentoIdStr = f.TaDominioModelTipoDocumento!.Descripcion,
                    NumeroIdentificacion = f.NumeroIdentificacion,
                    FechaExpedicion = f.FechaExpedicion,
                    Correo = f.Correo,
                    Telefono = f.Telefono,
                    Direccion = f.Direccion,
                    FechaNacimiento = f.FechaNacimiento,
                    GeneroId = f.GeneroId,
                    GeneroIdStr = f.TaDominioModelTipoDocumento!.Descripcion,
                    TerminosCondiciones = f.TerminosCondiciones,
                    PoliticaTratamientoDatos = f.PoliticaTratamientoDatos,
                    MayorEdad = f.MayorEdad,
                    ResponsabilidadFiscalId = f.ResponsabilidadFiscalId,
                    ResponsabilidadFiscalIdStr = f.TaDominioModelResponsabilidad!.Descripcion
                }).FirstOrDefaultAsync();

            if (resultado != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultado, typeof(RPersonaDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        #endregion
    }
}
