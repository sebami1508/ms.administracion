using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaErrorModelEtc : IEntityTypeConfiguration<TaErrorModel>
    {
        public void Configure(EntityTypeBuilder<TaErrorModel> builder)
        {
            builder.ToTable("TA_ERROR", "SC_ADMINISTRACION");

            builder.HasKey(e => e.ErrorId);

            builder.Property(e => e.ErrorId)
                .HasColumnName("ERROR_ID")
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(e => e.Identificador)
                .HasColumnName("IDENTIFICADOR")
                .HasMaxLength(8)
                .IsRequired();

            builder.Property(e => e.CodigoEstado)
                .HasColumnName("CODIGO_ESTADO")
                .HasMaxLength(4)
                .IsRequired();

            builder.Property(e => e.Mensaje)
                .HasColumnName("MENSAJE")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Excepcion)
                .HasColumnName("EXCEPCION")
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(e => e.Fecha)
                .HasColumnName("FECHA")
                .IsRequired();

            builder.Property(e => e.Modulo)
                .HasColumnName("MODULO")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.InnerException)
                .HasColumnName("INNER_EXCEPTION")
                .HasMaxLength(4000);

            builder.Property(e => e.StackTrace)
                .HasColumnName("STACK_TRACE")
                .HasMaxLength(4000);
        }
    }
}
