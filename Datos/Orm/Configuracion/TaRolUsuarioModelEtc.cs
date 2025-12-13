using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaRolUsuarioModelEtc : IEntityTypeConfiguration<TaRolUsuarioModel>
    {
        public void Configure(EntityTypeBuilder<TaRolUsuarioModel> builder)
        {

            builder.ToTable("TA_ROL_USUARIO", "SC_ADMINISTRACION");

            builder.HasKey(s => s.RolUsuarioId);

            builder.Property(s => s.RolUsuarioId)
                   .HasColumnName("ROL_USUARIO_ID");

            builder.Property(s => s.UsuarioId)
                   .HasColumnName("USUARIO_ID");

            builder.Property(s => s.RolId)
                   .HasColumnName("ROL_ID");

            builder.HasOne(s => s.TaRolModel)
               .WithMany(s => s.LtsTaRolesModel)
               .HasForeignKey(s => s.RolId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaUsuarioModel)
               .WithMany(s => s.LtsTaRolesModel)
               .HasForeignKey(s => s.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
