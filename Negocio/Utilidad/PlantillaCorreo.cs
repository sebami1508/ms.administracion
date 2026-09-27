using System.Globalization;
using System.Net;
using System.Text;

namespace Negocio.Utilidad
{
    /// <summary>
    /// Correos transaccionales con la identidad de Bradamela: la misma paleta del
    /// POS y de la App (morado de marca con acento dorado).
    ///
    /// Cada correo sale en HTML y en texto plano; el texto plano lo muestran los
    /// clientes sin HTML y ayuda a que el correo no termine en spam. El logo va
    /// embebido como cid:marca (Imagenes/Logos/logo_blanco.png, ver
    /// UtilidadesLogica.EnviarCorreo). Todo dato que venga del usuario se codifica
    /// antes de insertarlo en el HTML.
    /// </summary>
    public static class PlantillaCorreo
    {
        public sealed record Correo(string Html, string Texto);

        private const string Fuente = "'Montserrat',Arial,Helvetica,sans-serif";
        private const string FuenteMono = "Consolas,Menlo,'Courier New',monospace";

        // ---- Correos ----

        public static Correo CodigoRegistro(string? nombres, string codigo, int minutos, string? municipio)
        {
            const string intro = "Estás a un paso de crear tu cuenta en Bradamela. Ingresa este código en la aplicación para confirmar tu correo:";
            const string aviso = "¿No fuiste tú? Ignora este mensaje: sin el código nadie puede registrarse con tu correo.";

            return CorreoCodigo(
                titulo: "Confirma tu correo",
                etiqueta: "Verificación de correo",
                preheader: $"Usa este código para completar tu registro. Vence en {minutos} minutos.",
                nombres, intro, codigo, minutos, aviso, municipio);
        }

        public static Correo CodigoRestablecimiento(string? nombres, string codigo, int minutos, string? municipio)
        {
            const string intro = "Recibimos una solicitud para restablecer la contraseña de tu cuenta. Ingresa este código para crear una nueva:";
            const string aviso = "¿No lo solicitaste? Ignora este mensaje: tu contraseña actual sigue funcionando.";

            return CorreoCodigo(
                titulo: "Restablece tu contraseña",
                etiqueta: "Restablecer contraseña",
                preheader: $"Usa este código para crear una nueva contraseña. Vence en {minutos} minutos.",
                nombres, intro, codigo, minutos, aviso, municipio);
        }

        public static Correo UsuarioCreado(string? nombres, decimal? identificacion, string password, string urlInicio, string? municipio)
        {
            const string intro = "Creamos tu usuario en Bradamela. Estas son tus credenciales para ingresar por primera vez:";
            const string aviso = "Si no esperabas este correo, comunícate con el administrador.";

            return CorreoCredenciales(
                titulo: "¡Te damos la bienvenida!",
                etiqueta: "Cuenta nueva",
                preheader: "Tu usuario está listo. Aquí tienes tus credenciales de acceso.",
                nombres, intro, identificacion, password, urlInicio, aviso, municipio);
        }

        public static Correo PasswordRestablecida(string? nombres, decimal? identificacion, string password, string urlInicio, string? municipio)
        {
            const string intro = "Un administrador restableció la contraseña de tu cuenta. Usa estas credenciales para volver a ingresar:";
            const string aviso = "Si no pediste este cambio, comunícate con el administrador.";

            return CorreoCredenciales(
                titulo: "Tu nueva contraseña temporal",
                etiqueta: "Contraseña restablecida",
                preheader: "Te enviamos una contraseña temporal para ingresar a Bradamela.",
                nombres, intro, identificacion, password, urlInicio, aviso, municipio);
        }

        // ---- Tipos de correo ----

        private static Correo CorreoCodigo(string titulo, string etiqueta, string preheader,
            string? nombres, string intro, string codigo, int minutos, string aviso, string? municipio)
        {
            var saludo = Saludo(nombres);
            var vence = $"Vence en {minutos} minutos · Un solo uso";

            var cuerpo = new StringBuilder()
                .Append(Parrafo(Codificar(saludo), negrita: true))
                .Append(Parrafo(Codificar(intro)))
                .Append($$"""
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:26px 0 0;border-collapse:separate;">
                      <tr>
                        <td align="center" bgcolor="#F3EEF8" style="background:#F3EEF8;border:1px solid #E4D8EC;border-radius:18px;padding:22px 16px 20px;">
                          <div style="font-family:{{Fuente}};font-size:11px;font-weight:700;letter-spacing:2px;text-transform:uppercase;color:#7A3A8E;">Tu código</div>
                          <div class="bm-codigo" style="margin-top:8px;padding-left:12px;font-family:{{Fuente}};font-size:40px;line-height:1.15;font-weight:800;letter-spacing:12px;color:#4E1D66;">{{Codificar(codigo)}}</div>
                          <table role="presentation" align="center" cellpadding="0" cellspacing="0" style="margin:14px auto 0;">
                            <tr>
                              <td bgcolor="#FFF1CC" style="background:#FFF1CC;border-radius:999px;padding:6px 14px;font-family:{{Fuente}};font-size:12px;font-weight:600;color:#7A4E00;">{{Codificar(vence)}}</td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                    """)
                .Append(Aviso("Protege tu cuenta", "Bradamela nunca te pedirá este código por teléfono, WhatsApp ni redes sociales. No lo compartas con nadie."))
                .Append(Nota(Codificar(aviso)))
                .ToString();

            var texto = new StringBuilder()
                .AppendLine(saludo).AppendLine()
                .AppendLine(intro).AppendLine()
                .AppendLine($"Tu código: {codigo}")
                .AppendLine(vence).AppendLine()
                .AppendLine("Bradamela nunca te pedirá este código por teléfono, WhatsApp ni redes sociales. No lo compartas con nadie.").AppendLine()
                .AppendLine(aviso)
                .Append(PieTexto(municipio))
                .ToString();

            return new Correo(Layout(titulo, preheader, etiqueta, cuerpo, municipio), texto);
        }

        private static Correo CorreoCredenciales(string titulo, string etiqueta, string preheader,
            string? nombres, string intro, decimal? identificacion, string password, string urlInicio, string aviso, string? municipio)
        {
            var saludo = Saludo(nombres);
            var usuario = identificacion?.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
            var pasos = new[]
            {
                "Ingresa con tu número de identificación y la contraseña temporal.",
                "Cámbiala por una que solo tú conozcas.",
                "Verás los módulos que correspondan a los roles que te asignaron.",
            };

            var cuerpo = new StringBuilder()
                .Append(Parrafo(Codificar(saludo), negrita: true))
                .Append(Parrafo(Codificar(intro)))
                .Append($$"""
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:26px 0 0;border:1px solid #E4D8EC;border-radius:18px;border-collapse:separate;">
                      <tr>
                        <td style="padding:18px 22px;border-bottom:1px solid #F0E6F6;">
                          <div style="font-family:{{Fuente}};font-size:11px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#9A93A6;">Usuario</div>
                          <div style="margin-top:4px;font-family:{{Fuente}};font-size:19px;font-weight:700;color:#231A2E;">{{Codificar(usuario)}}</div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:18px 22px;">
                          <div style="font-family:{{Fuente}};font-size:11px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#9A93A6;">Contraseña temporal</div>
                          <div style="margin-top:8px;"><span style="display:inline-block;background:#FFF1CC;border-radius:10px;padding:7px 14px;font-family:{{FuenteMono}};font-size:20px;font-weight:700;letter-spacing:2px;color:#4E1D66;">{{Codificar(password)}}</span></div>
                        </td>
                      </tr>
                    </table>
                    """)
                .Append(Boton("Iniciar sesión", urlInicio))
                .Append(Pasos(pasos))
                .Append(Aviso("Información confidencial", "Tus credenciales son personales. No las compartas con nadie; Bradamela nunca te las pedirá."))
                .Append(Nota(Codificar(aviso)))
                .Append(Nota($"""¿El botón no funciona? Copia este enlace en tu navegador:<br><a href="{Codificar(urlInicio)}" style="color:#7A3A8E;word-break:break-all;">{Codificar(urlInicio)}</a>"""))
                .ToString();

            var texto = new StringBuilder()
                .AppendLine(saludo).AppendLine()
                .AppendLine(intro).AppendLine()
                .AppendLine($"Usuario: {usuario}")
                .AppendLine($"Contraseña temporal: {password}").AppendLine()
                .AppendLine($"Inicia sesión en: {urlInicio}").AppendLine()
                .AppendLine("Primeros pasos:");
            for (var i = 0; i < pasos.Length; i++)
                texto.AppendLine($"{i + 1}. {pasos[i]}");
            texto.AppendLine()
                .AppendLine("Tus credenciales son personales. No las compartas con nadie; Bradamela nunca te las pedirá.").AppendLine()
                .AppendLine(aviso)
                .Append(PieTexto(municipio));

            return new Correo(Layout(titulo, preheader, etiqueta, cuerpo, municipio), texto.ToString());
        }

        // ---- Estructura común ----

        /// <summary>
        /// Documento base: encabezado morado con el logo, tarjeta blanca y pie. Se
        /// arma con tablas y estilos en línea porque Outlook y Gmail ignoran casi
        /// todo el CSS moderno; los degradados tienen color de respaldo (bgcolor).
        /// </summary>
        private static string Layout(string titulo, string preheader, string etiqueta, string cuerpo, string? municipio)
        {
            var lugar = string.IsNullOrWhiteSpace(municipio)
                ? string.Empty
                : $"""<div style="margin-top:4px;font-family:{Fuente};font-size:12px;color:#9A93A6;">{Codificar(municipio)}</div>""";

            return $$"""
                <!DOCTYPE html>
                <html lang="es" xmlns="http://www.w3.org/1999/xhtml">
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1">
                  <meta name="x-apple-disable-message-reformatting">
                  <meta name="color-scheme" content="light">
                  <meta name="supported-color-schemes" content="light">
                  <title>{{Codificar(titulo)}}</title>
                  <link href="https://fonts.googleapis.com/css2?family=Montserrat:wght@400;600;700;800&display=swap" rel="stylesheet">
                  <style>
                    body { margin:0; padding:0; width:100% !important; background:#F8F5FB; }
                    table { border-collapse:collapse; }
                    img { border:0; outline:none; text-decoration:none; }
                    @media (max-width:620px) {
                      .bm-contenedor { width:100% !important; }
                      .bm-px { padding-left:22px !important; padding-right:22px !important; }
                      .bm-titulo { font-size:22px !important; }
                      .bm-codigo { font-size:32px !important; letter-spacing:8px !important; padding-left:8px !important; }
                    }
                  </style>
                </head>
                <body style="margin:0;padding:0;background:#F8F5FB;">
                  <div style="display:none;max-height:0;max-width:0;overflow:hidden;opacity:0;mso-hide:all;">{{Codificar(preheader)}}{{string.Concat(Enumerable.Repeat("&#847;&zwnj;&nbsp;", 40))}}</div>
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" bgcolor="#F8F5FB" style="background:#F8F5FB;">
                    <tr>
                      <td align="center" style="padding:28px 12px 32px;">
                        <table role="presentation" class="bm-contenedor" width="600" cellpadding="0" cellspacing="0" style="width:600px;max-width:600px;">
                          <tr>
                            <td>
                              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="border-collapse:separate;border-radius:24px;box-shadow:0 14px 36px rgba(78,29,102,0.12);">
                                <tr>
                                  <td class="bm-px" align="center" bgcolor="#4E1D66" style="background-color:#4E1D66;background-image:linear-gradient(135deg,#2A0E3D 0%,#4E1D66 38%,#7A3A8E 78%,#9B45C4 100%);border-radius:24px 24px 0 0;padding:34px 40px 36px;">
                                    <img src="cid:marca" width="230" alt="Bradamela" style="display:block;width:230px;max-width:80%;height:auto;margin:0 auto;font-family:{{Fuente}};font-size:28px;font-weight:800;color:#FFFFFF;">
                                    <table role="presentation" align="center" cellpadding="0" cellspacing="0" style="margin:20px auto 0;border-collapse:separate;">
                                      <tr>
                                        <td style="border:1px solid #F4B942;border-radius:999px;padding:6px 14px;font-family:{{Fuente}};font-size:11px;font-weight:700;letter-spacing:2px;text-transform:uppercase;color:#FFD27A;">{{Codificar(etiqueta)}}</td>
                                      </tr>
                                    </table>
                                    <h1 class="bm-titulo" style="margin:14px 0 0;font-family:{{Fuente}};font-size:26px;line-height:1.3;font-weight:800;color:#FFFFFF;">{{Codificar(titulo)}}</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td class="bm-px" bgcolor="#FFFFFF" style="background:#FFFFFF;border-radius:0 0 24px 24px;padding:32px 40px 34px;">
                {{cuerpo}}
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                          <tr>
                            <td align="center" style="padding:24px 24px 0;">
                              <div style="font-family:{{Fuente}};font-size:13px;font-weight:700;color:#4E1D66;">Bradamela · Somos artesanales</div>
                              {{lugar}}
                              <div style="margin-top:12px;font-family:{{Fuente}};font-size:11px;line-height:1.6;color:#9A93A6;">Mensaje automático. Por favor, no respondas a este correo.</div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
        }

        private static string Parrafo(string html, bool negrita = false)
        {
            var estilo = negrita
                ? "margin:0 0 12px;font-size:17px;font-weight:700;color:#231A2E;"
                : "margin:0;font-size:15px;line-height:1.65;color:#4A4257;";
            return $"""<p style="font-family:{Fuente};{estilo}">{html}</p>""";
        }

        private static string Boton(string texto, string url) => $$"""
            <table role="presentation" align="center" cellpadding="0" cellspacing="0" style="margin:28px auto 0;border-collapse:separate;">
              <tr>
                <td align="center" bgcolor="#7A3A8E" style="border-radius:999px;background-color:#7A3A8E;background-image:linear-gradient(135deg,#9B45C4 0%,#7A3A8E 55%,#4E1D66 100%);box-shadow:0 8px 20px rgba(122,58,142,0.30);">
                  <a href="{{Codificar(url)}}" target="_blank" style="display:inline-block;padding:15px 38px;font-family:{{Fuente}};font-size:15px;font-weight:700;color:#FFFFFF;text-decoration:none;border-radius:999px;">{{Codificar(texto)}}</a>
                </td>
              </tr>
            </table>
            """;

        private static string Pasos(IReadOnlyList<string> pasos)
        {
            var filas = new StringBuilder();
            for (var i = 0; i < pasos.Count; i++)
            {
                filas.Append($$"""
                    <tr>
                      <td width="38" valign="top" style="padding:0 0 12px;">
                        <div style="width:26px;height:26px;line-height:26px;border-radius:50%;background:#F0E6F6;text-align:center;font-family:{{Fuente}};font-size:13px;font-weight:800;color:#7A3A8E;">{{i + 1}}</div>
                      </td>
                      <td valign="top" style="padding:3px 0 12px;font-family:{{Fuente}};font-size:14px;line-height:1.55;color:#4A4257;">{{Codificar(pasos[i])}}</td>
                    </tr>
                    """);
            }

            return $$"""
                <div style="margin-top:32px;font-family:{{Fuente}};font-size:12px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;color:#4E1D66;">Primeros pasos</div>
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-top:14px;">
                {{filas}}
                </table>
                """;
        }

        private static string Aviso(string titulo, string texto) => $$"""
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:26px 0 0;border-collapse:separate;">
              <tr>
                <td bgcolor="#FFF8E6" style="background:#FFF8E6;border-left:4px solid #F4B942;border-radius:12px;padding:14px 18px;font-family:{{Fuente}};font-size:13px;line-height:1.55;color:#7A4E00;">
                  <strong>{{Codificar(titulo)}}.</strong> {{Codificar(texto)}}
                </td>
              </tr>
            </table>
            """;

        private static string Nota(string html) =>
            $"""<p style="margin:18px 0 0;font-family:{Fuente};font-size:13px;line-height:1.6;color:#6B6478;">{html}</p>""";

        private static string PieTexto(string? municipio)
        {
            var pie = new StringBuilder()
                .AppendLine().AppendLine("—")
                .AppendLine("Bradamela · Somos artesanales");
            if (!string.IsNullOrWhiteSpace(municipio))
                pie.AppendLine(municipio);
            return pie.AppendLine("Mensaje automático. Por favor, no respondas a este correo.").ToString();
        }

        // ---- Utilidades ----

        private static string Codificar(string? texto) => WebUtility.HtmlEncode(texto ?? string.Empty);

        /// <summary>Los nombres se guardan en mayúsculas; en el saludo van como nombre propio.</summary>
        private static string Saludo(string? nombres)
        {
            if (string.IsNullOrWhiteSpace(nombres))
                return "¡Hola!";

            var cultura = CultureInfo.GetCultureInfo("es-CO");
            var nombre = cultura.TextInfo.ToTitleCase(nombres.Trim().ToLower(cultura));
            return $"¡Hola, {nombre}!";
        }
    }
}
