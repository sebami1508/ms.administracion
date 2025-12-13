using Datos.Orm.Configuracion;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Datos.Orm.Contexto
{
    public class DbContextError: DbContext
    {
        public DbContextError()
        {
        }

        public DbContextError(DbContextOptions<DbContextError> options)
            : base(options)
        {
        }

        #region DbSets

        public virtual DbSet<TaErrorModel> TaError { get; set; }

        #endregion

        #region Configuraciones
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TaErrorModelEtc());
        }

        #endregion
    }
}
