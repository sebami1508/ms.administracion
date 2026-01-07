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
        #endregion

    }
}
