using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaTurnoModelEtc : IEntityTypeConfiguration<TaTurnoModel>
    {
        public void Configure(EntityTypeBuilder<TaTurnoModel> builder)
        {
            builder.ToTable("TA_TURNO", "SC_ADMINISTRACION");
            builder.HasKey(s => s.TurnoId);

            builder.Property(s => s.TurnoId)
                   .HasColumnName("TURNO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.UsuarioId)
                   .HasColumnName("USUARIO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.FechaTurno)
                   .HasColumnName("FECHA_TURNO");

            builder.Property(s => s.EstadoId)
                   .HasColumnName("ESTADO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Base)
                 .HasColumnName("BASE");

            builder.Property(s => s.FechaInicio)
                 .HasColumnName("FECHA_INICIO");

            builder.Property(s => s.FechaFin)
                 .HasColumnName("FECHA_FIN");

            builder.HasOne(s => s.TaUsuarioModel)
                    .WithMany(s => s.LtsTaTurnoModel)
                    .HasForeignKey(s => s.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModel)
                   .WithMany(s => s.LtsTaTurnoModel)
                   .HasForeignKey(s => s.EstadoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
