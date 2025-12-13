using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaAuditoriaModelEtc : IEntityTypeConfiguration<TaAuditoriaModel>
    {
        public void Configure(EntityTypeBuilder<TaAuditoriaModel> entity)
        {
            entity.HasKey(e => e.AuditoriaId).HasName("AUD_AUDITORIA_ID_PK");

            entity.ToTable("TA_AUDITORIA", "SC_ADMINISTRACION");

            entity.Property(e => e.AuditoriaId)
                .HasColumnName("AUDITORIA_ID")
                .HasMaxLength(40);

            entity.Property(e => e.Accion)
                .HasColumnName("ACCION")
                .HasMaxLength(15);

            entity.Property(e => e.Fecha)
                .HasColumnName("FECHA")
                .HasColumnType("DATETIME");

            entity.Property(e => e.MaquinaIp)
                .HasColumnName("MAQUINA_IP")
                .HasMaxLength(50);

            entity.Property(e => e.NombreTabla)
                .HasColumnName("NOMBRE_TABLA")
                .HasMaxLength(150);

            entity.Property(e => e.TablaId)
                .HasColumnName("TABLA_ID")
                .HasMaxLength(40);

            entity.Property(e => e.UsuarioId)
                .HasColumnName("USUARIO_ID")
                .HasMaxLength(40);

            entity.Property(e => e.ValorAntiguo)
                .HasColumnName("VALOR_ANTIGUO")
                .HasColumnType("VARCHAR(MAX)");

            entity.Property(e => e.ValorNuevo)
                .HasColumnName("VALOR_NUEVO")
                .HasColumnType("VARCHAR(MAX)");

            entity.Property(e => e.Modulo)
                .HasColumnName("MODULO")
                .HasMaxLength(150);

            entity.HasOne(d => d.TaUsuarioModel)
                .WithMany(p => p.LtsTaAuditoria)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("TA_AUDITORIA_TA_USUARIO_FK");

        }
    }
}
