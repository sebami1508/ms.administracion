using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaDispositivoFcmModelEtc : IEntityTypeConfiguration<TaDispositivoFcmModel>
    {
        public void Configure(EntityTypeBuilder<TaDispositivoFcmModel> entity)
        {
            entity.ToTable("TA_DISPOSITIVO_FCM", "SC_ADMINISTRACION");

            entity.HasKey(x => x.DispositivoId);

            entity.Property(x => x.DispositivoId).HasColumnName("DISPOSITIVO_ID").IsRequired();
            entity.Property(x => x.UsuarioId).HasColumnName("USUARIO_ID").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Token).HasColumnName("TOKEN").HasMaxLength(300).IsRequired();
            entity.Property(x => x.Plataforma).HasColumnName("PLATAFORMA").HasMaxLength(20);
            entity.Property(x => x.FechaRegistro).HasColumnName("FECHA_REGISTRO").IsRequired();

            entity.HasIndex(x => x.Token).IsUnique();
        }
    }
}
