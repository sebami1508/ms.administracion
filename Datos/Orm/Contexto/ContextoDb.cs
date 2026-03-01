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

        public DbSet<TaUsuarioModel> TaUsuarioModel { get; set; }
        public DbSet<TaRolModel> TaRolModel { get; set; }
        public DbSet<TaRolUsuarioModel> TaRolUsuarioModel { get; set; }
        public DbSet<TaMenuModel> TaMenuModel { get; set; }
        public DbSet<TaPerfilModel> TaPerfilModel { get; set; }
        public DbSet<TaZonaGeograficaModel> TaZonaGeograficaModel { get; set; }
        public DbSet<TaDominioModel> TaDominioModel { get; set; }
        public DbSet<TaProductoModel> TaProductoModel { get; set; }
        public DbSet<TaOrdenModel> TaOrdenModel { get; set; }
        public DbSet<TaItemModel> TaItemModel { get; set; }
        public DbSet<TaCaracteristicaModel> TaPizzaModel { get; set; }
        public DbSet<TaConsecutivoFacturaModel> TaConsecutivoFacturaModel { get; set; }
        public DbSet<TaTurnoModel> TaTurnoModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TaUsuarioModelEtc());
            modelBuilder.ApplyConfiguration(new TaRolModelEtc());
            modelBuilder.ApplyConfiguration(new TaRolUsuarioModelEtc());
            modelBuilder.ApplyConfiguration(new TaMenuModelEtc());
            modelBuilder.ApplyConfiguration(new TaPerfilModelEtc());
            modelBuilder.ApplyConfiguration(new TaZonaGeograficaModelEtc());
            modelBuilder.ApplyConfiguration(new TaDominioModelEtc());
            modelBuilder.ApplyConfiguration(new TaProductoModelEtc());
            modelBuilder.ApplyConfiguration(new TaOrdenModelEtc());
            modelBuilder.ApplyConfiguration(new TaItemModelEtc());
            modelBuilder.ApplyConfiguration(new TaItemModelEtc());
            modelBuilder.ApplyConfiguration(new TaCaracteristicaModelEtc());
            modelBuilder.ApplyConfiguration(new TaConsecutivoFacturaModelEtc());
            modelBuilder.ApplyConfiguration(new TaTurnoModelEtc());

            modelBuilder.Entity<TaOrdenModel>()
            .Property(x => x.FechaRegistro)
            .HasColumnType("timestamp without time zone");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var prop in entityType.GetProperties())
                {
                    var baseClr = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
                    if (baseClr == typeof(DateTime))
                        prop.SetColumnType("timestamp without time zone");
                }
            }
        }
    }
}
