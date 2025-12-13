using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaAuditoriaConsultaModelEtc : IEntityTypeConfiguration<TaAuditoriaConsultaModel>
    {
        public void Configure(EntityTypeBuilder<TaAuditoriaConsultaModel> entity)
        {
            entity.HasKey(e => e.AuditoriaConsultaId).HasName("TA_AUDITORIA_CONSULTA_PK");

            entity.ToTable("TA_AUDITORIA_CONSULTA", "SC_ADMINISTRACION");

            entity.Property(e => e.AuditoriaConsultaId)
                .HasColumnName("AUDITORIA_CONSULTA_ID")
                .HasMaxLength(40);

            entity.Property(e => e.Fecha)
               .HasColumnName("FECHA")
               .HasColumnType("DATE");

            entity.Property(e => e.MaquinaIp)
                .HasColumnName("MAQUINA_IP")
                .HasMaxLength(50);

            entity.Property(e => e.Metodo)
                .HasColumnName("METODO")
                .HasMaxLength(150);

            entity.Property(e => e.Modulo)
                .HasColumnName("MODULO")
                .HasMaxLength(150);

            entity.Property(e => e.Parametros)
                .HasColumnName("PARAMETROS")
                .HasColumnType("VARCHAR(MAX)");

            entity.Property(e => e.UsuarioId)
                 .HasColumnName("USUARIO_ID")
                .HasMaxLength(40);

            entity.HasOne(d => d.TaUsuarioModel)
                .WithMany(p => p.LtsTaAuditoriaConsulta)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("TA_AUDITORIA_CONSULTA_TA_USUARIO_FK");
        }
    }
}
