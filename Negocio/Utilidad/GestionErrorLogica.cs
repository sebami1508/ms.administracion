using Comun.Dto;
using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using System.Text.RegularExpressions;

namespace Negocio.Utilidad
{
    public class GestionErrorLogica
    {
        #region Atributos

        private readonly DbContextError db;

        #endregion

        #region Constructores

        public GestionErrorLogica(DbContextError _db)
        {
            db = _db;
        }

        #endregion

        #region Métodos

        public void SaveError(RespuestaDto<string> respuestaDto, string identificador, int codigoEstado)
        {
            try
            {
                string[] excepciones = respuestaDto.Respuesta.Split("=>");

                string excepcion = EliminarEspaciosEnBlanco(excepciones[0]);
                string innerException = EliminarEspaciosEnBlanco(excepciones[1]);
                string stackTrace = EliminarEspaciosEnBlanco(excepciones[2]);

                db.Add(new TaErrorModel
                {
                    ErrorId = Guid.NewGuid().ToString(),
                    Identificador = identificador,
                    CodigoEstado = codigoEstado.ToString(),
                    Mensaje = respuestaDto.Mensaje.Length >= 500 ? respuestaDto.Mensaje.Substring(0, 500) : respuestaDto.Mensaje,
                    Excepcion = excepcion.Length >= 4000 ? excepcion.Substring(0, 4000) : excepcion,
                    Fecha = DateTime.Now,
                    Modulo = Constantes.NombreMicroServicio,
                    InnerException = innerException.Length >= 4000 ? innerException.Substring(0, 4000) : innerException,
                    StackTrace = stackTrace.Length >= 4000 ? stackTrace.Substring(0, 4000) : stackTrace
                });

                db.SaveChanges();
            }
            catch { }
        }

        public string EliminarEspaciosEnBlanco(string cadena)
        {
            string texto = Regex.Replace(cadena, @"\r\n|\r|\n", " ");
            texto = Regex.Replace(texto, @" {2,}", " ");

            return texto.Trim();
        }

        #endregion

    }
}
