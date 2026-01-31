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
                <div style='font-family: Arial, Helvetica, sans-serif; color:#333; font-size:15px; line-height:1.6;'>

                    <div style='font-weight:bold; margin-top:10px;'>
                        {_myConfig.Municipio}, {fecha.ToLongDateString()}
                    </div>

                    <div style='margin-top:20px; font-weight:bold;'>
                        Estimado(a) {nombres} {apellidos},
                    </div>

                    <div style='margin-top:15px; text-align:justify;'>
                        Le informamos que se ha creado su usuario de acceso al sistema BRADAMELA.
                        A continuación encontrará sus credenciales iniciales:
                    </div>

                    <div style='margin-top:20px; padding:15px; border:1px solid #ccc; border-radius:6px; background:#f7f7f7;'>
                        <div><strong>Usuario:</strong> {identificacion}</div>
                        <div style='margin-top:8px;'><strong>Contraseña temporal:</strong> {password}</div>
                    </div>

                    <div style='margin-top:20px;'>
                        <a href='{urlInicio}'
                           style='display:inline-block; padding:10px 22px; background:#572364; color:#fff;
                                  text-decoration:none; border-radius:6px; font-weight:bold; text-transform:uppercase;'>
                            Iniciar sesión
                        </a>
                    </div>

                    <div style='margin-top:20px; text-align:justify;'>
                        El acceso al sistema se encuentra condicionado a los roles y permisos asignados.
                        Por motivos de seguridad, esta contraseña es de carácter temporal y deberá ser cambiada
                        una vez ingrese al sistema. Recuerde que esta información es personal y confidencial,
                        por lo que no debe compartirla con terceros.
                    </div>

                    <div style='margin-top:25px; font-size:12px; color:#666; text-align:center;'>
                        Este mensaje ha sido generado automáticamente por el sistema.
                        Por favor, no responda a este correo.
                    </div>

                </div>";
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
            var usuarioId = _param as string;
            string password = UtilidadesLogica.GenerarPassword();
            string urlInicio = _myConfig.UrlInicioSesion;

            var model = await db.TaUsuarioModel.FindAsync(usuarioId);

            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe el usuario.");

            DateTime fecha = DateTime.Now;
            var body = $@"
            <div style='font-family: Arial, Helvetica, sans-serif; color:#333; font-size: 15px; line-height: 1.6;'>

                <div style='margin-top: 10px; font-weight: bold;'>
                   {_myConfig.Municipio}, {fecha.ToLongDateString()}
                </div>

                <div style='margin-top: 20px; font-weight: bold;'>
                    Estimado(a) {model.Nombres} {model.Apellidos},
                </div>

                <div style='margin-top: 15px; text-align: justify;'>
                    Le informamos que su contraseña ha sido restablecida correctamente. A continuación,
                    encontrará las credenciales temporales de acceso. Por motivos de seguridad,
                    deberá cambiar la contraseña una vez inicie sesión en el sistema.
                </div>

                <div style='margin-top: 20px; padding: 15px; border: 1px solid #ccc; border-radius: 6px; background:#f7f7f7;'>
                    <div><strong>Usuario:</strong> {model.Identificacion}</div>
                    <div style='margin-top: 8px;'><strong>Contraseña temporal:</strong> {password}</div>
                </div>

                <div style='margin-top: 20px;'>
                    <a href='{urlInicio}'
                       style='display:inline-block; padding:10px 22px; background:#572364; color:#fff;
                              text-decoration:none; border-radius:6px; font-weight:bold; text-transform:uppercase;'>
                        Iniciar sesión
                    </a>
                </div>

                <div style='margin-top: 20px; text-align: justify;'>
                    El acceso al sistema se encuentra condicionado a los roles y permisos asignados.
                    Recuerde que esta información es confidencial y de uso personal; no debe ser compartida
                    bajo ninguna circunstancia.
                </div>

                <div style='margin-top: 25px; font-size: 12px; color:#666; text-align:center;'>
                    Este mensaje ha sido generado automáticamente por el sistema BRADAMELA POS.
                    Por favor, no responda a este correo.
                </div>

            </div>";


            var (exito, mensaje) = UtilidadesLogica.EnviarCorreo(_myConfig.CorreoNotificacion, _myConfig.PasswordCorreo, model.CorreoElectronico, "Notificación restablecimiento contraseña", body);

            if (!exito)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"No se logró enviar correo al usuario: {mensaje}");

            string passwordEncriptada = UtilidadesLogica.EncryptPassword(password, _myConfig.Key);

            model.Password = !string.IsNullOrEmpty(passwordEncriptada) ? passwordEncriptada : model.Password;
            model.IngresoPrimeraVez = true;
            db.Update(model);

            bool resultado = await db.SaveChangesAsync() > 0;

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

        public async Task<RespuestaDto<TReturn>> SolicitarOtpResetPasswordAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as SolicitarOtpResetDto;

            if (dto == null || string.IsNullOrWhiteSpace(dto.IdentificacionOCorreo))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Debe enviar la identificación o correo del usuario.");

            var criterio = dto.IdentificacionOCorreo.Trim().ToLower();

            // Busca por correo o identificación (ajusta tipo de Identificacion si es int)
            var usuario = await db.TaUsuarioModel.FirstOrDefaultAsync(u =>
                u.CorreoElectronico.ToLower() == criterio ||
                u.Identificacion.ToString() == criterio);

            if (usuario == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe un usuario con la identificación o correo proporcionado.");

            var otpsActivos = await db.Set<TaOtpModel>()
                .Where(x => x.UsuarioId == usuario.UsuarioId
                            && x.Proposito == "RESET_PASSWORD"
                            && !x.Usado
                            && x.FechaExpiracion > DateTime.UtcNow)
                .ToListAsync();

            if (otpsActivos.Count > 0)
            {
                foreach (var o in otpsActivos)
                {
                    o.Usado = true;
                    o.FechaUso = DateTime.UtcNow;
                }
                db.UpdateRange(otpsActivos);
            }

            // Genera OTP y guarda hash/salt
            var otp = OtpUtils.GenerarOtpNumerico(6);
            var (hash, salt) = OtpUtils.HashOtp(otp);

            var expiracionMin = 10;
            var modelOtp = new TaOtpModel
            {
                OtpId = Guid.NewGuid(),
                UsuarioId = usuario.UsuarioId,
                Proposito = "RESET_PASSWORD",
                OtpHash = hash,
                OtpSalt = salt,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(expiracionMin),
                Usado = false,
                Intentos = 0,
                MaxIntentos = 5,
                IpSolicitud = dto.Ip,
                UserAgent = dto.UserAgent
            };

            db.Add(modelOtp);

            var body = CrearCuerpoCorreoOtp(usuario.Nombres, usuario.Apellidos, otp, expiracionMin);
            var (exito, mensaje) = UtilidadesLogica.EnviarCorreo(
                _myConfig.CorreoNotificacion,
                _myConfig.PasswordCorreo,
                usuario.CorreoElectronico,
                "Código OTP para cambio de contraseña",
                body);

            if (!exito)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"No se logró enviar el OTP al correo: {mensaje}");

            var ok = await db.SaveChangesAsync() > 0;

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se logró registrar la solicitud OTP.");

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Se envió el código OTP al correo registrado.");
        }

        private string CrearCuerpoCorreoOtp(string nombres, string apellidos, string otp, int minutos)
        {
            var fecha = DateTime.Now;
            return $@"
            <div style='font-family: Arial, Helvetica, sans-serif; color:#333; font-size:15px; line-height:1.6;'>
                <div style='font-weight:bold; margin-top:10px;'>
                    {_myConfig.Municipio}, {fecha.ToLongDateString()}
                </div>

                <div style='margin-top:20px; font-weight:bold;'>
                    Estimado(a) {nombres} {apellidos},
                </div>

                <div style='margin-top:15px; text-align:justify;'>
                    Hemos recibido una solicitud para restablecer su contraseña.
                    Para continuar, ingrese el siguiente código OTP:
                </div>

                <div style='margin-top:20px; padding:15px; border:1px solid #ccc; border-radius:6px; background:#f7f7f7; font-size:22px; font-weight:bold; text-align:center; letter-spacing:4px;'>
                    {otp}
                </div>

                <div style='margin-top:15px; text-align:justify;'>
                    Este código expira en <strong>{minutos} minutos</strong> y solo puede usarse una vez.
                    Si usted no solicitó este cambio, ignore este mensaje.
                </div>

                <div style='margin-top:25px; font-size:12px; color:#666; text-align:center;'>
                    Este mensaje ha sido generado automáticamente por el sistema BRADAMELA POS.
                    Por favor, no responda a este correo.
                </div>
            </div>";
        }

        public async Task<RespuestaDto<TReturn>> ValidarOtpResetPasswordAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as ValidarOtpResetDto;

            if (dto == null || string.IsNullOrWhiteSpace(dto.IdentificacionOCorreo) || string.IsNullOrWhiteSpace(dto.Otp))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Debe enviar la identificación/correo y el OTP.");

            var criterio = dto.IdentificacionOCorreo.Trim().ToLower();
            var otpIngresado = dto.Otp.Trim();

            var usuario = await db.TaUsuarioModel.FirstOrDefaultAsync(u =>
                u.CorreoElectronico.ToLower() == criterio ||
                u.Identificacion.ToString() == criterio);

            if (usuario == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe un usuario con la identificación o correo proporcionado.");

            var otpDb = await db.Set<TaOtpModel>()
                .Where(x => x.UsuarioId == usuario.UsuarioId
                            && x.Proposito == "RESET_PASSWORD"
                            && !x.Usado)
                .OrderByDescending(x => x.FechaCreacion)
                .FirstOrDefaultAsync();

            if (otpDb == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe un OTP activo para este usuario.");

            // Expiración
            if (otpDb.FechaExpiracion <= DateTime.UtcNow)
            {
                otpDb.Usado = true;
                otpDb.FechaUso = DateTime.UtcNow;
                db.Update(otpDb);
                await db.SaveChangesAsync();

                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "El OTP ha expirado. Solicite uno nuevo.");
            }

            // Intentos
            if (otpDb.Intentos >= otpDb.MaxIntentos)
            {
                otpDb.Usado = true;
                otpDb.FechaUso = DateTime.UtcNow;
                db.Update(otpDb);
                await db.SaveChangesAsync();

                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Se superó el número máximo de intentos. Solicite un nuevo OTP.");
            }

            // Validación hash
            var valido = OtpUtils.ValidarOtp(otpIngresado, otpDb.OtpHash, otpDb.OtpSalt);

            otpDb.Intentos += 1;

            if (!valido)
            {
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "OTP inválido.");
            }

            // Consumir OTP
            otpDb.Usado = true;
            otpDb.FechaUso = DateTime.UtcNow;
            db.Update(otpDb);

            var ok = await db.SaveChangesAsync() > 0;

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se pudo completar la validación OTP.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "OTP validado correctamente.");
        }


        #endregion

    }
}
