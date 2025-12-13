using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Datos.Orm.Configuracion
{
    public class TaLegalModelEtc : IEntityTypeConfiguration<TaLegalModel>
    {
        public void Configure(EntityTypeBuilder<TaLegalModel> builder)
        {
            builder.ToTable("TA_LEGAL", "SC_ADMINISTRACION");

            builder.HasKey(s => s.RegistroId);

            builder.Property(s => s.RegistroId)
                   .HasColumnName("REGISTRO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.TipoLoteriaId)
                   .HasColumnName("TIPO_LOTERIA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.CiudadId)
                   .HasColumnName("CIUDAD_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.NumeroBillete)
                   .HasColumnName("NUMERO_BILLETE");

            builder.Property(s => s.NumeroSerie)
                   .HasColumnName("NUMERO_SERIE");

            builder.Property(s => s.NumeroSorteo)
                   .HasColumnName("NUMERO_SORTEO");

            builder.Property(s => s.DistribuidorId)
                   .HasColumnName("DISTRIBUIDOR_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.EstadoId)
                   .HasColumnName("ESTADO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Observacion)
                   .HasColumnName("OBSERVACION")
                   .HasMaxLength(500);

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");

            builder.Property(s => s.UsuarioId)
                   .HasColumnName("USUARIO_ID")
                   .HasMaxLength(40);

            builder.HasOne(s => s.TaDominioModelTipoLoteriaId)
                   .WithMany()
                   .HasForeignKey(s => s.TipoLoteriaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModelEstadoId)
                   .WithMany()
                   .HasForeignKey(s => s.EstadoId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDistribuidorModel)
                   .WithMany()
                   .HasForeignKey(s => s.DistribuidorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaUsuarioModel)
                   .WithMany()
                   .HasForeignKey(s => s.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
