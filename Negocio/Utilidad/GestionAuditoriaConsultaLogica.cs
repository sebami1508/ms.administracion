using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Negocio.Utilidad
{
    public class GestionAuditoriaConsultaLogica
    {
        #region Atributos

        private readonly DbContext db;

        #endregion

        #region Constructores

        public GestionAuditoriaConsultaLogica(DbContext _db)
        {
            db = _db;
        }

        #endregion

        #region Métodos

        public void AuditQuery(ParametrosAuditoriaDto _parametrosAuditoriaDto, [CallerFilePath] string _clase = null, [CallerMemberName] string _metodo = null, object? _parametros = null)
        {
            db.Add(new TaAuditoriaConsultaModel
            {
                AuditoriaConsultaId = Guid.NewGuid().ToString(),
                Modulo = Constantes.NombreMicroServicio,
                Metodo = $"{Path.GetFileNameWithoutExtension(_clase)}.{_metodo}",
                Parametros = JsonSerializer.Serialize(_parametros),
                Fecha = DateTime.Now,
                MaquinaIp = _parametrosAuditoriaDto.AuditoriaMaquina,
                UsuarioId = _parametrosAuditoriaDto.AuditoriaUsuario
            });

            db.SaveChanges();
        }

        #endregion
    }
}
