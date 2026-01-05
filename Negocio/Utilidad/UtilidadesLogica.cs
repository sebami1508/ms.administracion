using Comun.Dto;
using Comun.Enumeracion;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;

namespace Negocio.Utilidad
{
    public static class UtilidadesLogica
    {

        public static RespuestaDto<string> ConsolidacionMensajes(List<string> _ValidacionCampos, List<string> _ValidacionPersonalizada)
        {
            string mensajesCampos = _ValidacionCampos.Any() ? "Valide los siguientes campos: " + string.Join(", ", _ValidacionCampos) : string.Empty;
            string mensajesPersonalizados = _ValidacionPersonalizada.Any() ? string.Join(" / ", _ValidacionPersonalizada) : string.Empty;
            string mensajeResultado = string.Empty;

            if (!string.IsNullOrWhiteSpace(mensajesCampos) && !string.IsNullOrWhiteSpace(mensajesPersonalizados))
                mensajeResultado = $"{mensajesCampos} / {mensajesPersonalizados}";
            else if (!string.IsNullOrWhiteSpace(mensajesCampos))
                mensajeResultado = mensajesCampos;
            else
                mensajeResultado = mensajesPersonalizados;

            if (_ValidacionCampos.Count > 0 || _ValidacionPersonalizada.Count > 0)
                return new RespuestaDto<string>(EstadoOperacion.Malo, mensajeResultado);

            return new RespuestaDto<string>(EstadoOperacion.Bueno, string.Empty);
        }

        public static string Decrypt(string encryptedText, string key, string iv)
        {
            // Convertir el IV y la clave de Base64 a bytes
            byte[] ivBytes = Convert.FromBase64String(iv);  // El IV debe ser de 16 bytes
            byte[] buffer = Convert.FromBase64String(encryptedText);  // Decodificar el texto cifrado de Base64
            byte[] keyBytes = Convert.FromBase64String(key);  // Decodificar la clave de Base64

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;  // Usar la clave decodificada
                aes.IV = ivBytes;    // Usar el IV decodificado

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();  // Retornar el texto desencriptado
                        }
                    }
                }
            }
        }

        public static string GenerarPassword()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateKey()
        {
            SymmetricAlgorithm symmetricAlgorithm = new RijndaelManaged();
            symmetricAlgorithm.KeySize = 128;
            symmetricAlgorithm.GenerateKey();

            return Convert.ToBase64String(symmetricAlgorithm.Key);
        }

        public static string EncryptPassword(string message, string key)
        {
            SymmetricAlgorithm symmetricAlgorithm = new RijndaelManaged();
            symmetricAlgorithm.KeySize = 128;
            symmetricAlgorithm.Key = Convert.FromBase64String(key);
            symmetricAlgorithm.Mode = CipherMode.ECB;

            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateEncryptor();

            byte[] data = Encoding.UTF8.GetBytes(message);

            var dataEncrypted = cryptoTransform.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(dataEncrypted);
        }


        public static string DecryptPassword(string message, string key)
        {
            SymmetricAlgorithm symmetricAlgorithm = new RijndaelManaged();
            symmetricAlgorithm.KeySize = 128;
            symmetricAlgorithm.Key = Convert.FromBase64String(key);
            symmetricAlgorithm.Mode = CipherMode.ECB;

            ICryptoTransform cryptoTransform = symmetricAlgorithm.CreateDecryptor();
            string mensajeSinEspacios = message.Replace(" ", "");
            byte[] data = Convert.FromBase64String(mensajeSinEspacios);
            byte[] dataDecrypted = cryptoTransform.TransformFinalBlock(data, 0, data.Length);

            return Encoding.UTF8.GetString(dataDecrypted);
        }

        public static (bool, string) EnviarCorreo(string _correo, string _contraseña, string _destinario, string _asunto, string _body)
        {
            if (string.IsNullOrWhiteSpace(_correo) || string.IsNullOrWhiteSpace(_contraseña) ||
                string.IsNullOrWhiteSpace(_destinario) || string.IsNullOrWhiteSpace(_asunto) || string.IsNullOrWhiteSpace(_body))
            {
                return (false, "Los datos para enviar el correo están incompletos.");
            }

            try
            {
                var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagenes\\Logos\\logo_03.png");

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_correo);
                    mailMessage.Subject = _asunto;
                    mailMessage.Body = _body;
                    mailMessage.IsBodyHtml = true;

                    var htmlView = AlternateView.CreateAlternateViewFromString(_body, null, "text/html");

                    if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                    {
                        var logo = new LinkedResource(logoPath, "image/png")
                        {
                            ContentId = "logo",
                            TransferEncoding = TransferEncoding.Base64
                        };
                        htmlView.LinkedResources.Add(logo);
                    }

                    mailMessage.AlternateViews.Add(htmlView);
                    mailMessage.To.Add(_destinario);

                    using (var client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(_correo, _contraseña);
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        client.Send(mailMessage);
                    }
                }

                return (true, "Correo enviado correctamente.");
            }
            catch (SmtpException smtpEx)
            {
                return (false, $"Error SMTP: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error general: {ex.Message}");
            }
        }




        public static (bool, string) EnviarCorreoAdjuntos(string correo, string contraseña, string destinario, string asunto, string body, byte[] archivo1 = null, byte[] archivo2 = null)
        {
            using (var client = new SmtpClient("smtp.office365.com", 587))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(correo, contraseña);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(correo);
                    mailMessage.To.Add(destinario);
                    mailMessage.Subject = asunto;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    // Ruta de la imagen
                    //var logoPath = "D:\\1_Proyectos\\Ms.Uniox\\WebApi\\Imagenes\\Logos\\logo_03.png";
                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagenes\\Logos\\logo_03.png");

                    // Crear vista alternativa HTML
                    var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                    // Verificar si la ruta de la imagen es válida y si el archivo existe
                    if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                    {
                        // Incluir la imagen en el mensaje
                        var logo = new LinkedResource(logoPath, "image/png")
                        {
                            ContentId = "logo",
                            TransferEncoding = TransferEncoding.Base64
                        };

                        // Agregar la imagen a la vista HTML
                        htmlView.LinkedResources.Add(logo);
                    }

                    // Agregar la vista alternativa al correo
                    mailMessage.AlternateViews.Add(htmlView);

                    // Adjuntar primer archivo
                    if (archivo1 != null)
                    {
                        var stream1 = new MemoryStream(archivo1);
                        var attachment1 = new Attachment(stream1, "Carta de instrucciones.pdf", MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(attachment1);

                    }

                    // Adjuntar segundo archivo
                    if (archivo2 != null)
                    {
                        var stream2 = new MemoryStream(archivo2);
                        var attachment2 = new Attachment(stream2, "Certificado de ahorro.pdf", MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(attachment2);

                    }

                    try
                    {
                        client.Send(mailMessage);
                        return (true, "Se envío el correo de manera correcta.");
                    }
                    catch (SmtpException smtpEx)
                    {
                        // Manejo de excepciones SMTP específicas
                        return (false, $"Error SMTP: {smtpEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Manejo de cualquier otra excepción
                        return (false, $"Error general: {ex.Message}");
                    }
                }
            }
        }

        public static (bool, string) EnviarCorreoOneAdjuntos(string correo, string contraseña, string destinario, string asunto, string body, byte[] archivo = null)
        {
            using (var client = new SmtpClient("smtp.office365.com", 587))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(correo, contraseña);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(correo);
                    mailMessage.To.Add(destinario);
                    mailMessage.Subject = asunto;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;

                    var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imagenes\\Logos\\logo_03.png");

                    // Crear vista alternativa HTML
                    var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                    // Verificar si la ruta de la imagen es válida y si el archivo existe
                    if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                    {
                        // Incluir la imagen en el mensaje
                        var logo = new LinkedResource(logoPath, "image/png")
                        {
                            ContentId = "logo",
                            TransferEncoding = TransferEncoding.Base64
                        };

                        // Agregar la imagen a la vista HTML
                        htmlView.LinkedResources.Add(logo);
                    }

                    // Agregar la vista alternativa al correo
                    mailMessage.AlternateViews.Add(htmlView);

                    // Adjuntar el archivo sin usar 'using' para evitar el cierre prematuro del stream
                    if (archivo != null)
                    {
                        var stream1 = new MemoryStream(archivo); // No se usa 'using'
                        var attachment1 = new Attachment(stream1, "Carta de instrucciones.pdf", MediaTypeNames.Application.Pdf);
                        mailMessage.Attachments.Add(attachment1);
                    }

                    try
                    {
                        client.Send(mailMessage);
                        return (true, "Se envió el correo de manera correcta.");
                    }
                    catch (SmtpException smtpEx)
                    {
                        return (false, $"Error SMTP: {smtpEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        return (false, $"Error general: {ex.Message}");
                    }
                }
            }
        }
    }
}




