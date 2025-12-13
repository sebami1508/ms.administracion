using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaRolModelEtc : IEntityTypeConfiguration<TaRolModel>
    {
        public void Configure(EntityTypeBuilder<TaRolModel> builder)
        {

            builder.ToTable("TA_ROL", "SC_ADMINISTRACION");

            builder.HasKey(s => s.RolId);

            builder.Property(s => s.RolId)
                   .HasColumnName("ROL_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Descripcion)
                   .HasColumnName("DESCRIPCION")
                   .HasMaxLength(150);

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");
        }
    }
}
