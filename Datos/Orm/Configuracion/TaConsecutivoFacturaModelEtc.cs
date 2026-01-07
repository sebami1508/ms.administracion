using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaConsecutivoFacturaModelEtc : IEntityTypeConfiguration<TaConsecutivoFacturaModel>
    {
        public void Configure(EntityTypeBuilder<TaConsecutivoFacturaModel> entity)
        {
            entity.ToTable("TA_CONSECUTIVO_FACTURA", "SC_ADMINISTRACION");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .HasColumnName("ID");

            entity.Property(x => x.Prefijo)
                .HasColumnName("PREFIJO")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(x => x.Anio)
                .HasColumnName("ANIO")
                .IsRequired();

            entity.Property(x => x.UltimoNumero)
                .HasColumnName("ULTIMO_NUMERO")
                .IsRequired();

            entity.Property(x => x.FechaActualizacion)
                .HasColumnName("FECHA_ACTUALIZACION")
                .IsRequired();

            entity.HasIndex(x => new { x.Prefijo, x.Anio })
                .IsUnique()
                .HasDatabaseName("UQ_CONSECUTIVO");

        }
    }
}
