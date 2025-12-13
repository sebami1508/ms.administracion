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

namespace Negocio.Gestion
{
    public class DistribuidorLogica : IDistribuidor
    {
        private readonly ContextoDb db;
        private readonly CDistribuidorValidator validatorC;
        private readonly UDistribuidorValidator validatorU;
        public readonly MyConfig _myConfig;

        public DistribuidorLogica(ContextoDb _db, MyConfig myConfig)
        {
            db = _db;
            validatorC = new CDistribuidorValidator();
            validatorU = new UDistribuidorValidator();
            _myConfig = myConfig;
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CDistribuidorDto;
            var respuestaValidacion = await validatorC.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {

                var existente = await db.TaDistribuidorModel.FirstOrDefaultAsync(x => x.TipoIdentificacionId == dto.TipoIdentificacionId && x.NumeroIdentificacion == dto.NumeroIdentificacion);

                if (existente != null)
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un distribuidor con el mismo tipo y número de identificación.");

                var model = new TaDistribuidorModel
                {
                    DistribuidorId = Guid.NewGuid().ToString(),
                    Nombre = dto.Nombre!.Trim().ToUpper(),
                    TipoIdentificacionId = dto.TipoIdentificacionId!,
                    NumeroIdentificacion = dto.NumeroIdentificacion,
                    Direccion = dto.Direccion!.Trim().ToUpper(),
                    PersonaContacto = dto.PersonaContacto!.Trim().ToUpper(),
                    Telefono = dto.Telefono,
                    Correo = dto.Correo!.Trim().ToLower(),
                    Vigente = true
                };

                await db.TaDistribuidorModel.AddAsync(model);
                bool resultado = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

                if (!resultado)
                {
                    await transaction.RollbackAsync();
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No fue posible guardar el distribuidor.");
                }

                // Crear usuario
                var (okUsuario, mensajeUsuario) = await CrearUsuarioAsync(dto);

                if (!okUsuario)
                {
                    await transaction.RollbackAsync();

                    var mensajeError = string.IsNullOrWhiteSpace(mensajeUsuario)
                        ? "No fue posible crear el usuario. Los cambios han sido revertidos."
                        : mensajeUsuario;

                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, mensajeError);
                }

                // Si todo bien en BD, confirmar transacción
                await transaction.CommitAsync();

                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, mensajeUsuario);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, $"Error al guardar el distribuidor: {ex.Message}");
            }
        }

        public async Task<(bool ok, string mensaje)> CrearUsuarioAsync(CDistribuidorDto dto)
        {
            var nombreNormalizados = dto.Nombre!.Trim().ToUpper();
            var correoNormalizado = dto.Correo!.Trim().ToLower();

            var usuarioExistente = await db.TaUsuarioModel
                .FirstOrDefaultAsync(a =>
                    a.Nombres == nombreNormalizados || a.Identificacion == dto.NumeroIdentificacion || a.CorreoElectronico == correoNormalizado);

            if (usuarioExistente != null)
            {
                if (usuarioExistente.Nombres == nombreNormalizados)
                {
                    return (false, "No es posible guardar el distribuidor, porque existe un usuario con el mismo Nombre.");
                }

                if (usuarioExistente.Identificacion == dto.NumeroIdentificacion)
                {
                    return (false, "No es posible guardar el distribuidor, porque existe un usuario con el mismo número de identificación.");
                }

                if (usuarioExistente.CorreoElectronico == correoNormalizado)
                {
                    return (false, "No es posible guardar el distribuidor, porque existe un usuario con el mismo correo electrónico.");
                }
            }


            string password = UtilidadesLogica.GenerarPassword();
            string passwordEncriptada = UtilidadesLogica.EncryptPassword(password, _myConfig.Key);

            var nuevoUsuario = new TaUsuarioModel
            {
                UsuarioId = Guid.NewGuid().ToString(),
                Nombres = nombreNormalizados,
                Identificacion = (decimal)dto.NumeroIdentificacion!,
                Celular = dto.Telefono.ToString(),
                CorreoElectronico = correoNormalizado,
                Password = passwordEncriptada,
                IngresoPrimeraVez = true,
                Vigente = true,
                Externo = true
            };

            db.Add(nuevoUsuario);
            bool resultadoUsuario = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (!resultadoUsuario)
            {
                return (false, "No es posible guardar el distribuidor, porque no se logró crear el usuario.");
            }

            var rolUsuario = new TaRolUsuarioModel
            {
                RolUsuarioId = Guid.NewGuid().ToString(),
                UsuarioId = nuevoUsuario.UsuarioId,
                RolId = Constantes.RolDistribuidor
            };

            db.Add(rolUsuario);
            bool resultadoRol = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (!resultadoRol)
            {
                return (false, "No es posible guardar el distribuidor, porque no se logró asignar el rol al usuario.");
            }

            var cuerpoCorreo = CrearCuerpoCorreo(
                nombreNormalizados,
                password,
                dto.NumeroIdentificacion,
                _myConfig.UrlInicioSesion
            );

            var (exitoCorreo, mensajeCorreo) = UtilidadesLogica.EnviarCorreo(
                _myConfig.CorreoNotificacion,
                _myConfig.PasswordCorreo,
                correoNormalizado,
                "Notificación creación de usuario",
                cuerpoCorreo
            );

            if (!exitoCorreo)
                return (false, $"No es posible guardar el distribuidor, porque no se logró enviar el correo de notificación de la creación del usuario: {mensajeCorreo}");

            return (true, "El distribuidor fue creado correctamente y se envió la notificación por correo electrónico las credenciales de acceso al portal.");
        }


        private string CrearCuerpoCorreo(string nombres, string password, decimal? identificacion, string urlInicio)
        {
            var fecha = DateTime.Now;
            var fechaLarga = fecha.ToString("D", new System.Globalization.CultureInfo("es-CO"));
            var identificacionTexto = identificacion?.ToString("0") ?? string.Empty;

            return $@"
                <div style='font-size:16px; font-family:Arial; color:#333; text-align:left; margin-top:20px;'>
                    Bogotá D.C., {fechaLarga}
                </div>

                <br />

                <div style='font-size:16px; font-family:Arial; color:#333; font-weight:bold; margin-top:10px;'>
                    Estimado(a) {nombres}
                </div>

                <div style='font-size:15px; font-family:Arial; color:#333; text-align:justify; margin-top:20px;'>
                    Queremos informarle que su usuario ha sido creado exitosamente en el portal transaccional de la Lotería de la Cruz Roja.
                    Para ingresar, deberá autenticarse con los siguientes datos:
                </div>

                <br />

                <div style='font-size:16px; font-family:Arial; color:#333; margin-top:15px;'>
                    <strong>Usuario:</strong> {identificacionTexto}
                </div>

                <div style='font-size:16px; font-family:Arial; color:#333; margin-top:10px;'>
                    <strong>Contraseña:</strong> {password}
                </div>

                <br />

                <div style='font-size:15px; font-family:Arial; color:#333; text-align:justify; margin-top:20px;'>
                    Por su seguridad, tenga en cuenta las siguientes recomendaciones:
                    <ul style='margin-top:10px; font-size:15px; color:#333;'>
                        <li>No comparta su contraseña con nadie.</li>
                        <li>Utilice contraseñas difíciles de adivinar y cámbielas periódicamente.</li>
                        <li>Evite ingresar al sistema desde redes Wi-Fi públicas.</li>
                        <li>Si recibe correos sospechosos solicitando información personal, ignórelos y repórtelos.</li>
                        <li>Verifique siempre que la URL de acceso sea la oficial del sistema.</li>
                    </ul>
                </div>

                <div style='margin-top:20px; margin-bottom:20px;'>
                    <a href='{urlInicio}'
                       style='display:inline-block; text-decoration:none; font-size:16px; font-family:Arial; color:#ffffff;
                              background-color:#e30615; border:2px solid #e30615; padding:10px 20px; font-weight:bold;
                              cursor:pointer; border-radius:5px; text-transform:uppercase;'>
                        Iniciar sesión
                    </a>
                </div>

                <div style='margin-top:30px; text-align:center;'>
                    <img src='cid:logo' alt='Logo' style='width:300px; height:auto;' />
                </div>

                <div style='font-family:Arial; font-size:12px; color:#666; text-align:center; margin-top:20px;'>
                    *Por favor, no responda a este mensaje. Ha sido generado automáticamente por el sistema.
                </div>";
        }


        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UDistribuidorDto;
            var respuestaValidacion = await validatorU.ValidateAsync(dto);
            if (!respuestaValidacion.IsValid)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = respuestaValidacion.ToString(),
                };
            }

            var model = await db.TaDistribuidorModel.FindAsync(dto.DistribuidorId);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El distribuidor no existe.",
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
            var model = await db.TaDistribuidorModel.FindAsync(dto.Id);
            if (model == null)
            {
                return new RespuestaDto<TReturn>
                {
                    Codigo = EstadoOperacion.Validacion,
                    Mensaje = "El distribuidor no existe.",
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
            var resultados = await db.TaDistribuidorModel.Select(f => new RDistribuidorDto
            {
                DistribuidorId = f.DistribuidorId,
                Nombre = f.Nombre,
                TipoIdentificacionId = f.TipoIdentificacionId,
                TipoIdentificacionStr = f.TaDominioModel.Descripcion,
                NumeroIdentificacion = f.NumeroIdentificacion,
                Direccion = f.Direccion,
                PersonaContacto = f.PersonaContacto,
                Telefono = f.Telefono,
                Correo = f.Correo,
                Vigente = f.Vigente
            }).OrderBy(o => o.Nombre).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RDistribuidorDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarPorNumeroIdentificacionAsync<TParam, TReturn>(TParam _param)
        {
            var numero = _param as decimal?;
            if (numero == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificación inválida.");

            var resultado = await db.TaDistribuidorModel.Where(x => x.NumeroIdentificacion == numero)
                .Select(f => new RDistribuidorDto
                {
                    DistribuidorId = f.DistribuidorId,
                    Nombre = f.Nombre,
                    TipoIdentificacionId = f.TipoIdentificacionId,
                    TipoIdentificacionStr = f.TaDominioModel.Descripcion,
                    NumeroIdentificacion = f.NumeroIdentificacion,
                    Direccion = f.Direccion,
                    PersonaContacto = f.PersonaContacto,
                    Telefono = f.Telefono,
                    Correo = f.Correo,
                    Vigente = f.Vigente
                }).FirstOrDefaultAsync();

            if (resultado != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultado, typeof(RDistribuidorDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }
    }
}
