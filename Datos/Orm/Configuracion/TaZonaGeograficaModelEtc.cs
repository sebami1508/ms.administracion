using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaZonaGeograficaModelEtc : IEntityTypeConfiguration<TaZonaGeograficaModel>
    {
        public void Configure(EntityTypeBuilder<TaZonaGeograficaModel> builder)
        {
            builder.ToTable("TA_ZONA_GEOGRAFICA", "SC_ADMINISTRACION");

            builder.HasKey(s => s.ZonaGeograficaId);

            builder.Property(s => s.ZonaGeograficaId)
                   .HasColumnName("ZONA_GEOGRAFICA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Descripcion)
                   .HasColumnName("DESCRIPCION")
                   .HasMaxLength(80);

            builder.Property(s => s.CodigoDane)
                   .HasColumnName("CODIGO_DANE");

            builder.Property(s => s.Longitud)
                   .HasColumnName("LONGITUD")
                   .HasMaxLength(25);

            builder.Property(s => s.Latitud)
                   .HasColumnName("LATITUD")
                   .HasMaxLength(25);

            builder.Property(s => s.PadreId)
                   .HasColumnName("PADRE_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.CodigoIso)
                   .HasColumnName("CODIGO_ISO")
                   .HasMaxLength(45);

            builder.HasOne(s => s.Padre)
                    .WithMany(s => s.LtsZonaGeografica)
                    .HasForeignKey(s => s.PadreId)
                    .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
