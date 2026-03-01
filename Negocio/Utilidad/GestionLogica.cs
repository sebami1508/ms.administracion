using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Reflection;

namespace Negocio.Utilidad
{
    public class GestionLogica
    {
        #region Atributos

        private readonly DbContext db;

        private static readonly HashSet<string> CamposProtegidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "FechaRegistro"
        };

        #endregion

        #region Constructores

        public GestionLogica(DbContext _db)
        {
            db = _db;
        }

        #endregion

        #region Métodos

        public void ActualizarCamposAutomatico<TModel, TParam>(TParam dto, TModel modelo)
        {
            var camposParaActualizar = IdentificarCamposActualizar(dto);

            var propiedadesModelo = typeof(TModel)
                .GetProperties()
                .ToDictionary(p => p.Name, p => p);

            var formatosFecha = new[]
            {
                "dd/MM/yyyy hh:mm:ss tt",
                "dd/MM/yyyy h:mm:ss tt",
                "d/MM/yyyy h:mm:ss tt",
                "dd/MM/yyyy HH:mm:ss",
                "d/MM/yyyy HH:mm:ss"
            };

            var cultureCO = new CultureInfo("es-CO");

            static DateTime ToUnspecified(DateTime dt) =>
                dt.Kind == DateTimeKind.Unspecified ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

            foreach (var item in camposParaActualizar)
            {
                if (CamposProtegidos.Contains(item.Key))
                    continue;

                if (!propiedadesModelo.TryGetValue(item.Key, out var propiedad))
                    continue;

                if (!propiedad.CanWrite)
                    continue;

                var raw = item.Value?.ToString();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var tipoBase = Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType;

                object? valorConvertido = null;

                if (tipoBase == typeof(DateTime))
                {
                    TimeZoneInfo tzCO;
                    try
                    {
                        tzCO = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                    }
                    catch
                    {
                        tzCO = TimeZoneInfo.Local; 
                    }

                    DateTime? dtFinal = null;

                    if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dtoff))
                    {
                        var dtCO = TimeZoneInfo.ConvertTime(dtoff, tzCO).DateTime;
                        dtFinal = DateTime.SpecifyKind(dtCO, DateTimeKind.Unspecified);
                    }
                    else if (DateTime.TryParseExact(raw, formatosFecha, cultureCO, DateTimeStyles.None, out var parsedExact))
                    {
                        dtFinal = DateTime.SpecifyKind(parsedExact, DateTimeKind.Unspecified);
                    }
                    else if (DateTime.TryParse(raw, cultureCO, DateTimeStyles.None, out var parsed))
                    {
                        dtFinal = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
                    }
                    else
                    {
                        continue;
                    }

                    if (dtFinal.Value <= new DateTime(1900, 1, 1))
                        continue;

                    valorConvertido = propiedad.PropertyType == typeof(DateTime)
                        ? dtFinal.Value
                        : (DateTime?)dtFinal.Value;
                }
                else
                {
                    try
                    {
                        valorConvertido = Convert.ChangeType(raw, tipoBase, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        continue;
                    }
                }


                if (valorConvertido != null)
                    propiedad.SetValue(modelo, valorConvertido);
            }

            db.Update(modelo);
        }

        private Dictionary<string, string> IdentificarCamposActualizar<TParam>(TParam objeto)
        {
            Dictionary<string, string> camposParaActualizar = new Dictionary<string, string>();

            // Obtener todos los campos de la clase
            var properties = typeof(TParam).GetProperties();

            // Recorrer cada campo e imprimir su nombre y valor
            foreach (PropertyInfo property in properties)
            {
                var valor = property.GetValue(objeto)?.ToString();

                if (!string.IsNullOrEmpty(valor))
                    camposParaActualizar.Add(property.Name, valor);
            }

            return camposParaActualizar;
        }
        #endregion

    }
}
