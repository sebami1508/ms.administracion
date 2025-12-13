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

        #endregion

        #region Constructores

        public GestionAuditoriaLogica(DbContext _db)
        {
            db = _db;
        }

        #endregion

        #region Métodos

        public void ActualizarCamposAutomatico<TModel, TParam>(TParam _dto, TModel _modelo)
        {
            var camposParaActualizar = IdentificarCamposActualizar(_dto);

            var propiedadesModelo = typeof(TModel).GetProperties().ToDictionary(prop => prop.Name, prop => prop);

            foreach (var item in camposParaActualizar)
            {
                if (propiedadesModelo.TryGetValue(item.Key, out var propiedad))
                {
                    object? valorConvertido = null;

                    if (propiedad.PropertyType == typeof(DateTime?) || propiedad.PropertyType == typeof(DateTime))
                    {
                        var formatosFecha = new[] { "dd/MM/yyyy hh:mm:ss tt", "dd/MM/yyyy h:mm:ss tt", "d/MM/yyyy h:mm:ss tt" };
                        var cultureInfo = new CultureInfo("es-CO");

                        if (DateTime.TryParseExact(item.Value.ToString(), formatosFecha, cultureInfo, DateTimeStyles.None, out DateTime parsedDate))
                            valorConvertido = (DateTime?)parsedDate;
                    }
                    else
                    {
                        valorConvertido = Convert.ChangeType(item.Value, Nullable.GetUnderlyingType(propiedad.PropertyType) ?? propiedad.PropertyType);
                    }

                    if (valorConvertido != null)
                        propiedad.SetValue(_modelo, valorConvertido);
                }
            }

            db.Update(_modelo);
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
                        Fecha = DateTime.Now,
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
