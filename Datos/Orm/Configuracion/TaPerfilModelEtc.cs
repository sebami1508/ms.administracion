using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaPerfilModelEtc : IEntityTypeConfiguration<TaPerfilModel>
    {
        public void Configure(EntityTypeBuilder<TaPerfilModel> builder)
        {
            builder.ToTable("TA_PERFILES", "SC_ADMINISTRACION");

            builder.HasKey(s => s.PerfilId);

            builder.Property(s => s.PerfilId)
                   .HasColumnName("PERFIL_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.MenuId)
                   .HasColumnName("MENU_ID")
                   .HasMaxLength(40)
                   .IsRequired();

            builder.Property(s => s.RolId)
                   .HasColumnName("ROL_ID")
                   .HasMaxLength(40)
                   .IsRequired();

            builder.HasOne(p => p.TaMenuModel)
                   .WithMany(m => m.LtsTaPerfilModel)
                   .HasForeignKey(p => p.MenuId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.TaRolModel)
                   .WithMany(r => r.LtsTaPerfilModel)
                   .HasForeignKey(p => p.RolId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
