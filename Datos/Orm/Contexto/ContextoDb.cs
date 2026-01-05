using Datos.Orm.Configuracion;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Datos.Orm.Contexto
{
    public class ContextoDb : DbContext
    {
        public ContextoDb(DbContextOptions<ContextoDb> options) : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }

        public DbSet<TaAuditoriaModel> TaAuditoria { get; set; }
        public DbSet<TaUsuarioModel> TaUsuarioModel { get; set; }
        public DbSet<TaRolModel> TaRolModel { get; set; }
        public DbSet<TaRolUsuarioModel> TaRolUsuarioModel { get; set; }
        public DbSet<TaMenuModel> TaMenuModel { get; set; }
        public DbSet<TaPerfilModel> TaPerfilModel { get; set; }
        public DbSet<TaZonaGeograficaModel> TaZonaGeograficaModel { get; set; }
        public DbSet<TaDominioModel> TaDominioModel { get; set; }
        public DbSet<TaProductoModel> TaProductoModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TaAuditoriaModelEtc());
            modelBuilder.ApplyConfiguration(new TaUsuarioModelEtc());
            modelBuilder.ApplyConfiguration(new TaRolModelEtc());
            modelBuilder.ApplyConfiguration(new TaRolUsuarioModelEtc());
            modelBuilder.ApplyConfiguration(new TaMenuModelEtc());
            modelBuilder.ApplyConfiguration(new TaPerfilModelEtc());
            modelBuilder.ApplyConfiguration(new TaZonaGeograficaModelEtc());
            modelBuilder.ApplyConfiguration(new TaDominioModelEtc());
            modelBuilder.ApplyConfiguration(new TaProductoModelEtc());
        }

    }
}
