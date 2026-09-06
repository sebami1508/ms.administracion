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

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            else
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        /// <summary>
        /// Registro de cliente desde la app móvil: crea el usuario con la
        /// contraseña elegida y le asigna automáticamente el rol CLIENTE.
        /// </summary>
        private const string RolClienteId = "5791103e-11ed-4c6a-9a87-63fcaf3c046c";
        private const int OtpRegistroExpiracionMin = 10;

        /// <summary>
        /// Envía un OTP de 6 dígitos al correo para verificarlo antes del registro
        /// (evita registros automatizados / robots).
        /// </summary>
        public async Task<RespuestaDto<TReturn>> SolicitarOtpRegistroAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as SolicitarOtpRegistroDto;

            if (dto == null || string.IsNullOrWhiteSpace(dto.CorreoElectronico) || !dto.CorreoElectronico.Contains('@'))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El correo electrónico no es válido.");

            var correoNormalizado = dto.CorreoElectronico.Trim().ToLower();

            var yaRegistrado = await db.TaUsuarioModel.AnyAsync(u => u.CorreoElectronico == correoNormalizado);
            if (yaRegistrado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con el mismo correo electrónico.");

            // Invalida OTPs de registro activos para este correo.
            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            var otpsActivos = await db.TaOtpRegistroModel
                .Where(x => x.Correo == correoNormalizado && !x.Usado && x.FechaExpiracion > ahora)
                .ToListAsync();

            foreach (var o in otpsActivos)
            {
                o.Usado = true;
                o.FechaUso = ahora;
            }
            if (otpsActivos.Count > 0)
                db.UpdateRange(otpsActivos);

            var otp = OtpUtils.GenerarOtpNumerico(6);
            var (hash, salt) = OtpUtils.HashOtp(otp);

            db.Add(new TaOtpRegistroModel
            {
                OtpRegistroId = Guid.NewGuid(),
                Correo = correoNormalizado,
                OtpHash = hash,
                OtpSalt = salt,
                FechaCreacion = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                FechaExpiracion = DateTime.SpecifyKind(DateTime.Now.AddMinutes(OtpRegistroExpiracionMin), DateTimeKind.Unspecified),
                Usado = false,
                Intentos = 0,
                MaxIntentos = 5
            });

            var nombre = string.IsNullOrWhiteSpace(dto.Nombres) ? "cliente" : dto.Nombres.Trim();
            var body = CrearCuerpoCorreoOtpRegistro(nombre, otp, OtpRegistroExpiracionMin);
            var (exito, mensaje) = UtilidadesLogica.EnviarCorreo(
                _myConfig.CorreoNotificacion,
                _myConfig.PasswordCorreo,
                correoNormalizado,
                "Código de verificación para su registro",
                body);

            if (!exito)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"No se logró enviar el código al correo: {mensaje}");

            var ok = await db.SaveChangesAsync() > 0;

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se logró registrar la solicitud del código.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Se envió el código de verificación al correo indicado.");
        }

        private string CrearCuerpoCorreoOtpRegistro(string nombre, string otp, int minutos)
        {
            var fecha = DateTime.Now;
            return $@"
            <div style='font-family: Arial, Helvetica, sans-serif; color:#333; font-size:15px; line-height:1.6;'>
                <div style='font-weight:bold; margin-top:10px;'>
                    {_myConfig.Municipio}, {fecha.ToLongDateString()}
                </div>

                <div style='margin-top:20px; font-weight:bold;'>
                    Estimado(a) {nombre},
                </div>

                <div style='margin-top:15px; text-align:justify;'>
                    Para completar su registro en BRADAMELA, ingrese el siguiente
                    código de verificación en la aplicación:
                </div>

                <div style='margin-top:20px; padding:15px; border:1px solid #ccc; border-radius:6px; background:#f7f7f7; font-size:22px; font-weight:bold; text-align:center; letter-spacing:4px;'>
                    {otp}
                </div>

                <div style='margin-top:15px; text-align:justify;'>
                    Este código expira en <strong>{minutos} minutos</strong> y solo puede usarse una vez.
                    Si usted no solicitó este registro, ignore este mensaje.
                </div>

                <div style='margin-top:25px; font-size:12px; color:#666; text-align:center;'>
                    Este mensaje ha sido generado automáticamente por el sistema BRADAMELA POS.
                    Por favor, no responda a este correo.
                </div>
            </div>";
        }

        /// <summary>
        /// Valida y consume el OTP de registro asociado al correo.
        /// Devuelve null si es válido, o el mensaje de error.
        /// </summary>
        private async Task<string?> ValidarOtpRegistroAsync(string correoNormalizado, string codigoOtp)
        {
            var otpDb = await db.TaOtpRegistroModel
                .Where(x => x.Correo == correoNormalizado && !x.Usado)
                .OrderByDescending(x => x.FechaCreacion)
                .FirstOrDefaultAsync();

            if (otpDb == null)
                return "No existe un código de verificación activo para este correo. Solicite uno nuevo.";

            if (otpDb.FechaExpiracion <= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified))
            {
                otpDb.Usado = true;
                otpDb.FechaUso = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return "El código de verificación ha expirado. Solicite uno nuevo.";
            }

            if (otpDb.Intentos >= otpDb.MaxIntentos)
            {
                otpDb.Usado = true;
                otpDb.FechaUso = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return "Se superó el número máximo de intentos. Solicite un nuevo código.";
            }

            var valido = OtpUtils.ValidarOtp(codigoOtp.Trim(), otpDb.OtpHash, otpDb.OtpSalt);

            otpDb.Intentos += 1;

            if (!valido)
            {
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return "El código de verificación es inválido.";
            }

            // Consumir OTP (se persiste junto con el registro del usuario).
            otpDb.Usado = true;
            otpDb.FechaUso = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            db.Update(otpDb);
            return null;
        }

        public async Task<RespuestaDto<TReturn>> RegistrarClienteAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CRegistroClienteDto;

            if (dto == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Objeto incompleto.");

            if (string.IsNullOrWhiteSpace(dto.Nombres) || string.IsNullOrWhiteSpace(dto.Apellidos))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Los nombres y apellidos son obligatorios.");

            if (dto.Identificacion == null || dto.Identificacion <= 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La identificación es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.Celular))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El celular es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.CorreoElectronico) || !dto.CorreoElectronico.Contains('@'))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El correo electrónico no es válido.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Trim().Length < 6)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La contraseña debe tener al menos 6 caracteres.");

            if (string.IsNullOrWhiteSpace(dto.CodigoOtp))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Debe ingresar el código de verificación enviado a su correo.");

            var nombresNormalizados = dto.Nombres.Trim().ToUpper();
            var apellidosNormalizados = dto.Apellidos.Trim().ToUpper();
            var correoNormalizado = dto.CorreoElectronico.Trim().ToLower();

            var usuarioExistente = await db.TaUsuarioModel
                .FirstOrDefaultAsync(a => a.Identificacion == dto.Identificacion || a.CorreoElectronico == correoNormalizado);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.Identificacion == dto.Identificacion)
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con el mismo número de identificación.");

                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un usuario con el mismo correo electrónico.");
            }

            // Verificación anti-robot: el correo debe haber sido validado con OTP.
            var errorOtp = await ValidarOtpRegistroAsync(correoNormalizado, dto.CodigoOtp);
            if (errorOtp != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, errorOtp);

            string passwordEncriptada = UtilidadesLogica.EncryptPassword(dto.Password.Trim(), _myConfig.Key);

            var nuevoUsuario = new TaUsuarioModel
            {
                UsuarioId = Guid.NewGuid().ToString(),
                Nombres = nombresNormalizados,
                Apellidos = apellidosNormalizados,
                Identificacion = dto.Identificacion.Value,
                Celular = dto.Celular.Trim(),
                CorreoElectronico = correoNormalizado,
                Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim(),
                Password = passwordEncriptada,
                IngresoPrimeraVez = false,
                Vigente = true
            };

            var rolCliente = new TaRolUsuarioModel
            {
                RolUsuarioId = Guid.NewGuid().ToString(),
                RolId = RolClienteId,
                UsuarioId = nuevoUsuario.UsuarioId
            };

            db.Add(nuevoUsuario);
            db.Add(rolCliente);

            // Un solo SaveChanges: usuario y rol se guardan de forma atómica.
            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Registro realizado correctamente. Ya puede iniciar sesión.");

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

            dto.Nombres = dto.Nombres!.Trim().ToUpperInvariant();
            dto.Apellidos = dto.Apellidos!.Trim().ToUpperInvariant();
            dto.CorreoElectronico = dto.CorreoElectronico!.Trim().ToLowerInvariant();

            new GestionLogica(db).ActualizarCamposAutomatico(dto, model);

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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

            bool resultado = await db.SaveChangesAsync(true) > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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
                    Direccion = f.Direccion,
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
                        Direccion = f.Direccion,
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

            bool resultado = await db.SaveChangesAsync() > 0;

            if (resultado)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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

            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            var otpsActivos = await db.Set<TaOtpModel>()
                .Where(x => x.UsuarioId == usuario.UsuarioId
                            && x.Proposito == "RESET_PASSWORD"
                            && !x.Usado
                            && x.FechaExpiracion > ahora)
                .ToListAsync();

            if (otpsActivos.Count > 0)
            {
                foreach (var o in otpsActivos)
                {
                    o.Usado = true;
                    o.FechaUso = ahora;
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
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddMinutes(expiracionMin),
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

        /// <summary>
        /// Valida el OTP de restablecimiento y, si es correcto, fija la nueva
        /// contraseña de forma atómica (valida + consume OTP + actualiza clave).
        /// </summary>
        public async Task<RespuestaDto<TReturn>> RestablecerPasswordConOtpAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as RestablecerPasswordOtpDto;

            if (dto == null
                || string.IsNullOrWhiteSpace(dto.IdentificacionOCorreo)
                || string.IsNullOrWhiteSpace(dto.Otp)
                || string.IsNullOrWhiteSpace(dto.NewPassword))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Debe enviar la identificación/correo, el OTP y la nueva contraseña.");

            if (dto.NewPassword.Trim().Length < 6)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La contraseña debe tener al menos 6 caracteres.");

            var criterio = dto.IdentificacionOCorreo.Trim().ToLower();
            var otpIngresado = dto.Otp.Trim();
            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

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
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No existe un OTP activo. Solicite uno nuevo.");

            if (otpDb.FechaExpiracion <= ahora)
            {
                otpDb.Usado = true;
                otpDb.FechaUso = ahora;
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "El OTP ha expirado. Solicite uno nuevo.");
            }

            if (otpDb.Intentos >= otpDb.MaxIntentos)
            {
                otpDb.Usado = true;
                otpDb.FechaUso = ahora;
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Se superó el número máximo de intentos. Solicite un nuevo OTP.");
            }

            var valido = OtpUtils.ValidarOtp(otpIngresado, otpDb.OtpHash, otpDb.OtpSalt);
            otpDb.Intentos += 1;

            if (!valido)
            {
                db.Update(otpDb);
                await db.SaveChangesAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "OTP inválido.");
            }

            // OTP válido: consumir y fijar la nueva contraseña en una sola operación.
            otpDb.Usado = true;
            otpDb.FechaUso = ahora;
            db.Update(otpDb);

            string passwordEncriptada = UtilidadesLogica.EncryptPassword(dto.NewPassword.Trim(), _myConfig.Key);
            usuario.Password = passwordEncriptada;
            usuario.IngresoPrimeraVez = false;
            db.Update(usuario);

            var ok = await db.SaveChangesAsync() > 0;

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se pudo restablecer la contraseña.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Contraseña restablecida correctamente.");
        }


        #endregion

    }
}
