using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaDominioModelEtc : IEntityTypeConfiguration<TaDominioModel>
    {
        public void Configure(EntityTypeBuilder<TaDominioModel> builder)
        {
            builder.ToTable("TA_DOMINIO", "SC_ADMINISTRACION");

            builder.HasKey(s => s.DominioId);

            builder.Property(s => s.DominioId)
                   .HasColumnName("DOMINIO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Descripcion)
                   .HasColumnName("DESCRIPCION")
                   .HasMaxLength(255);

            builder.Property(s => s.PadreId)
                   .HasColumnName("PADRE_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");

            builder.HasOne(s => s.Padre)
                    .WithMany(s => s.LtsHijos)
                    .HasForeignKey(s => s.PadreId)
                    .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
