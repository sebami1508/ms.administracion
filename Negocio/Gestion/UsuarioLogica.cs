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
    public class UsuarioLogica : IUsuario
    {
        #region Atributos

        private readonly ContextoDb db;
        private readonly CUsuarioValidator validacionC;
        private readonly UUsuarioValidator validacionU;
        public readonly MyConfig _myConfig;

        #endregion

        #region Constructores

        public UsuarioLogica(ContextoDb _db, MyConfig myConfig)
        {
            db = _db;
            validacionC = new CUsuarioValidator();
            validacionU = new UUsuarioValidator();
            _myConfig = myConfig;
        }
        #endregion

        #region Métodos

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CUsuarioDto;
            var respuestaValidacion = await validacionC.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var nombresNormalizados = dto.Nombres.Trim().ToUpper();
            var apellidosNormalizados = dto.Apellidos.Trim().ToUpper();
            var correoNormalizado = dto.CorreoElectronico.Trim().ToLower();

            var usuarioExistente = await db.TaUsuarioModel
                .FirstOrDefaultAsync(a => (a.Nombres == nombresNormalizados && a.Apellidos == apellidosNormalizados) ||
                                          a.Identificacion == dto.Identificacion || a.CorreoElectronico == correoNormalizado);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.Nombres == nombresNormalizados && usuarioExistente.Apellidos == apellidosNormalizados)
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con los mismos nombres y apellidos.");

                if (usuarioExistente.Identificacion == dto.Identificacion)
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con el mismo número de identificación.");

                if (usuarioExistente.CorreoElectronico == correoNormalizado)
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con el mismo correo electrónico.");
            }

            string password = UtilidadesLogica.GenerarPassword();
            string passwordEncriptada = UtilidadesLogica.EncryptPassword(password, _myConfig.Key);

            var cuerpoCorreo = CrearCuerpoCorreo(nombresNormalizados, apellidosNormalizados, dto.Identificacion, password, _myConfig.UrlInicioSesion);
            var (exito, mensaje) = UtilidadesLogica.EnviarCorreo(_myConfig.CorreoNotificacion, _myConfig.PasswordCorreo, correoNormalizado, "Notificación creación de usuario", cuerpoCorreo);

            if (!exito)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"No se logró enviar el correo al usuario: {mensaje}");

            var nuevoUsuario = new TaUsuarioModel
            {
                UsuarioId = Guid.NewGuid().ToString(),
                Nombres = nombresNormalizados,
                Apellidos = apellidosNormalizados,
                Identificacion = (int)dto.Identificacion,
                Celular = dto.Celular.Trim(),
                CorreoElectronico = correoNormalizado,
                Password = passwordEncriptada,
                IngresoPrimeraVez = false,
                Vigente = true
            };

            db.Add(nuevoUsuario);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        private string CrearCuerpoCorreo(string nombres, string apellidos, decimal? identificacion, string password, string urlInicio)
        {
            var fecha = DateTime.Now;
            return $@"
                <div style='font-size: 16px; font-family: Arial; font-style: italic; color: #333; font-weight: bold; text-align: left; margin-top: 20px;'>
                    Santa María - Huila, {fecha.ToLongDateString()}
                </div>
                <br />
                <div style='font-size: 16px; font-family: Arial; font-style: italic; color: #333; font-weight: bold; text-align: left; margin-top: 10px;'>
                    Estimado(a) {nombres} {apellidos}
                </div>
                <div style='font-size: 16px; font-family: Arial; color: #333; text-align: justify; font-weight: normal; margin-top: 20px;'>
                    Queremos informarle que le hemos creado un usuario para el sistema de BRADAMELA. A continuación, encontrará los detalles de acceso:
                </div>
                <br />
                <div style='font-size: 16px; font-family: Arial; color: #333; text-align: left; margin-top: 20px;'>
                    <strong>Usuario:</strong> {identificacion}
                </div>
                <div style='font-size: 16px; font-family: Arial; color: #333; text-align: left; margin-top: 10px;'>
                    <strong>Contraseña:</strong> {password}
                </div>
                <br />
                <br />
                <div style='font-size: 16px; font-family: Arial; color: #333; text-align: justify; margin-top: 20px;'>
                    Estos datos le permitirán acceder al sistema de acuerdo a los roles que le han sido asignados. Recuerde que esta información es confidencial, exclusivamente para usted y no debe compartirla con nadie más. Una vez ingrese al sistema deberá cambiar la contraseña.                
                </div>
                <div style='margin-top: 10px; margin-bottom: 10px;'>
                    <a href='{urlInicio}' style='display: inline-block; text-decoration: none; font-size: 16px; font-family: Arial; color: white; background-color: #572364; border: 2px solid #572364; padding: 10px 20px; font-weight: bold; cursor: pointer; transition: background-color 0.3s, color 0.3s, border-color 0.3s; border-radius: 5px; outline: none; text-transform: uppercase;'>
                        Iniciar sesión
                    </a>
                </div>
                <div style='margin-top: 30px;'>
                    <img src='cid:logo' alt='Logo' style='width: 300px; height: auto;' />
                </div>
                <div style='font-family: Arial; font-size: 12px; color: #666; text-align: center; margin-top: 20px;'>
                    *Por favor, no responda a este mensaje. Ha sido generado automáticamente por el sistema.
                </div>
            ";
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UUsuarioDto;
            var respuestaValidacion = await validacionU.ValidateAsync(dto);

            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaUsuarioModel.FindAsync(dto.UsuarioId);

            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El usuario no existe.",
                };
            }

            dto.Nombres = dto.Nombres.Trim().ToUpper();
            dto.Apellidos = dto.Apellidos.Trim().ToUpper();
            dto.CorreoElectronico = dto.CorreoElectronico.Trim().ToLower();

            GestionAuditoriaLogica auditoriaLogica = new GestionAuditoriaLogica(db);
            auditoriaLogica.ActualizarCamposAutomatico(dto, model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarVigenciaAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UUsuarioDto;

            var dato = await db.TaUsuarioModel.FindAsync(dto.UsuarioId);

            if (dato == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El usuario no existe.",
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

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as EliminarDto;

            if (string.IsNullOrWhiteSpace(dto.Id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe un identificador.");

            var dato = await db.TaUsuarioModel.FindAsync(dto.Id);

            if (dato == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El usuario no existe.",
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

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaUsuarioModel
                .Select(f => new RUsuarioDto
                {
                    UsuarioId = f.UsuarioId,
                    Nombres = f.Nombres,
                    Apellidos = f.Apellidos,
                    Identificacion = f.Identificacion,
                    Celular = f.Celular,
                    CorreoElectronico = f.CorreoElectronico,
                    Vigente = f.Vigente,
                    IngresoPrimeraVez = f.IngresoPrimeraVez

                }).OrderBy(o => o.Nombres).ToListAsync();

            if (resultados.Count() != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RUsuarioDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarUsuarioPorIdentificacionAnsync<TParam, TReturn>(TParam _param)
        {
            var strParam = _param as string;
            decimal.TryParse(strParam, out decimal identificacion);

            if (identificacion == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe una identificación para consultar el usuario.");

            var resultado = await db.TaUsuarioModel.Where(x => x.Identificacion == identificacion)

                .Select(f => new RUsuarioDatosDto
                {
                    Usuario = new RUsuarioDto
                    {
                        UsuarioId = f.UsuarioId,
                        Nombres = f.Nombres,
                        Apellidos = f.Apellidos,
                        Identificacion = f.Identificacion,
                        Celular = f.Celular,
                        CorreoElectronico = f.CorreoElectronico,
                        Vigente = f.Vigente,
                        IngresoPrimeraVez = f.IngresoPrimeraVez
                    },
                    Roles = db.TaRolUsuarioModel.Where(x => x.UsuarioId == f.UsuarioId && x.TaRolModel.Vigente == true).Select(r => new RRolUsuarioDto
                    {
                        RolUsuarioId = r.RolUsuarioId,
                        RolId = r.RolId,
                        UsuarioId = r.UsuarioId,
                        DescripcionRol = r.TaRolModel.Descripcion
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (resultado != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "No se encontró el usuario con la identificación suministrada.", (TReturn)Convert.ChangeType(resultado, typeof(RUsuarioDatosDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> EnviarNuevaPasswordAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UUsuarioDto;
            string password = UtilidadesLogica.GenerarPassword();
            string urlInicio = _myConfig.UrlInicioSesion;

            var model = await db.TaUsuarioModel.FindAsync(dto.UsuarioId);

            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe el usuario.");

            DateTime fecha = DateTime.Now;
            var body = $@"
                    <div style='font-size: 16px; font-family: Arial; font-style: italic; color: #333; font-weight: bold; text-align: left; margin-top: 20px;'>
                        Santa María - Huila, {fecha.ToLongDateString()}
                    </div>
                    <br />
                    <div style='font-size: 16px; font-family: Arial; font-style: italic; color: #333; font-weight: bold; text-align: left; margin-top: 10px;'>
                        Estimado(a) {model.Nombres} {model.Apellidos}
                    </div>
                    <div style='font-size: 16px; font-family: Arial; color: #333; text-align: justify; font-weight: normal; margin-top: 20px;'>
                        Queremos informarle que hemos restablecido su contraseña. A continuación, encontrará los detalles de la nueva contraseña, la cual deberá cambiar una vez ingrese al sistema:
                    </div>
                    <br />
                    <div style='font-size: 16px; font-family: Arial; color: #333; text-align: left; margin-top: 20px;'>
                        <strong>Usuario:</strong> {model.Identificacion}
                    </div>
                    <div style='font-size: 16px; font-family: Arial; color: #333; text-align: left; margin-top: 10px;'>
                        <strong>Contraseña:</strong> {password}
                    </div>
                    <br />
                     <div style='margin-top: 10px; margin-bottom: 10px;'>
                        <a href='{urlInicio}' style='display: inline-block; text-decoration: none; font-size: 16px; font-family: Arial; color: white; background-color: #572364; border: 2px solid #572364; padding: 10px 20px; font-weight: bold; cursor: pointer; transition: background-color 0.3s, color 0.3s, border-color 0.3s; border-radius: 5px; outline: none; text-transform: uppercase;'>
                            Iniciar sesión
                        </a>
                    </div>
                    <div style='font-size: 16px; font-family: Arial; color: #333; text-align: justify; margin-top: 20px;'>
                        Estos datos le permitirán acceder al sistema de acuerdo a los roles que le han sido asignados. Recuerde que esta información es confidencial, exclusivamente para usted y no debe compartirla con nadie más. Una vez ingrese al sistema deberá cambiar la contraseña.                </div>
                    </div>
                <div style='margin-top: 30px;'>
                        <img src='cid:logo' alt='Logo' style='width: 300px; height: auto;' />
                </div>
                <div style='font-family: Arial; font-size: 12px; color: #666; text-align: center; margin-top: 20px;'>
                    *Por favor, no responda a este mensaje. Ha sido generado automáticamente por el sistema de ASOMUFFAA UNIOX.                    
                </div>
            ";

            var (exito, mensaje) = UtilidadesLogica.EnviarCorreo(_myConfig.CorreoNotificacion, _myConfig.PasswordCorreo, model.CorreoElectronico, "Notificación restablecimiento contraseña", body);

            if (!exito)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"No se logró enviar correo al usuario: {mensaje}");

            string passwordEncriptada = UtilidadesLogica.EncryptPassword(password, _myConfig.Key);

            model.Password = !string.IsNullOrEmpty(passwordEncriptada) ? passwordEncriptada : model.Password;
            model.IngresoPrimeraVez = true;
            db.Update(model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");

        }

        public async Task<RespuestaDto<TReturn>> ActualizarPasswordAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UUsuarioDto;

            if (string.IsNullOrWhiteSpace(dto.Password))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe la contraseña actual.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword1))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No existe una nueva contraseña.");

            dto.Password?.Trim();

            var model = await db.TaUsuarioModel.FindAsync(dto.UsuarioId);

            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe el usuario.");

            string passwordActual = UtilidadesLogica.DecryptPassword(model.Password, _myConfig.Key);

            if (passwordActual != dto.Password)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "La contraseña actual es incorrecta, por favor inténtelo de nuevo.");

            dto.NewPassword1?.Trim();

            if (passwordActual == dto.NewPassword1)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "La nueva contraseña debe ser diferente a la actual, por favor inténtelo de nuevo.");

            string passwordEncriptada = UtilidadesLogica.EncryptPassword(dto.NewPassword1, _myConfig.Key);

            model.Password = !string.IsNullOrEmpty(passwordEncriptada) ? passwordEncriptada : model.Password;
            model.IngresoPrimeraVez = false;
            db.Update(model);

            bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");

        }
        #endregion

    }
}
