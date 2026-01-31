using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace Negocio.Utilidad
{
    public class GestionAuditoriaLogica
    {
        #region Atributos

        private readonly DbContext db;

        private static readonly HashSet<string> CamposProtegidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "FechaRegistro"
        };

        #endregion

        #region Constructores

        public GestionAuditoriaLogica(DbContext _db)
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

        public bool SaveChanges(ParametrosAuditoriaDto _parametrosAuditoriaDto)
        {
            AuditChanges(_parametrosAuditoriaDto);
            return db.SaveChanges() > 0;
        }

        public async Task<bool> SaveChangesAsync(ParametrosAuditoriaDto _parametrosAuditoriaDto, bool _actionDelete = false)
        {
            AuditChanges(_parametrosAuditoriaDto, _actionDelete);
            return await db.SaveChangesAsync() > 0;
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

        private void AuditChanges(ParametrosAuditoriaDto _parametrosAuditoriaDto, bool _actionDelete = false)
        {
            var entries = db.ChangeTracker.Entries().Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified
                          || e.State == EntityState.Deleted) && e.Metadata.GetTableName() != "ta_auditoria").ToList();

            foreach (var entry in entries)
            {
                string nombreTabla = entry.Metadata.GetTableName();
                string valorAntiguo = GetOldValues(entry);
                string valorNuevo = GetNewValues(entry);
                string primaryKey = GetPrimaryKeyValue(entry);

                if (valorAntiguo != valorNuevo)
                {
                    var auditEntry = new TaAuditoriaModel
                    {
                        AuditoriaId = Guid.NewGuid().ToString(),
                        TablaId = primaryKey,
                        Fecha = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                        MaquinaIp = _parametrosAuditoriaDto.AuditoriaMaquina,
                        UsuarioId = _parametrosAuditoriaDto.AuditoriaUsuario,
                        ValorAntiguo = valorAntiguo,
                        ValorNuevo = valorNuevo,
                        Accion = _actionDelete ? "Delete" : Enum.GetName(typeof(EntityState), entry.State),
                        NombreTabla = nombreTabla,
                        Modulo = Constantes.NombreMicroServicio
                    };

                    db.Add(auditEntry);
                }
            }
        }

        private string GetOldValues(EntityEntry entry)
        {
            Dictionary<string, object> oldValues = new Dictionary<string, object>();

            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                foreach (var property in entry.OriginalValues.Properties)
                {
                    string propertyName = property.Name;
                    object originalValue = entry.OriginalValues[property];
                    object currentValue = entry.CurrentValues[property];

                    if (!object.Equals(originalValue, currentValue))
                    {
                        oldValues.Add(propertyName, originalValue);
                    }
                }
            }

            if (oldValues.Count > 0)
                return JsonSerializer.Serialize(oldValues).ToString();
            return JsonSerializer.Serialize(new { Accion = Enum.GetName(typeof(EntityState), entry.State) }).ToString();
        }

        private string GetNewValues(EntityEntry entry)
        {
            Dictionary<string, object> newValues = new Dictionary<string, object>();

            if (entry.State == EntityState.Added)
            {
                foreach (var property in entry.CurrentValues.Properties)
                {
                    string propertyName = property.Name;
                    object currentValue = entry.CurrentValues[property];
                    newValues.Add(propertyName, currentValue);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var property in entry.CurrentValues.Properties)
                {
                    string propertyName = property.Name;
                    object originalValue = entry.OriginalValues[property];
                    object currentValue = entry.CurrentValues[property];

                    if (!object.Equals(originalValue, currentValue))
                    {
                        newValues.Add(propertyName, currentValue);
                    }
                }
            }

            if (newValues.Count > 0)
                return JsonSerializer.Serialize(newValues).ToString();
            return JsonSerializer.Serialize(new { Accion = Enum.GetName(typeof(EntityState), entry.State) }).ToString();
        }

        private string GetPrimaryKeyValue(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();
            var keyValues = key.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();

            return string.Join(",", keyValues);
        }

        #endregion

    }
}
