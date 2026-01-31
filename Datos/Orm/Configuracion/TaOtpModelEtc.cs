using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaOtpModelEtc : IEntityTypeConfiguration<TaOtpModel>
    {
        public void Configure(EntityTypeBuilder<TaOtpModel> entity)
        {
            entity.ToTable("TA_OTP", "SC_ADMINISTRACION");

            entity.HasKey(x => x.OtpId);

            entity.Property(x => x.OtpId).HasColumnName("OTP_ID").IsRequired();
            entity.Property(x => x.UsuarioId).HasColumnName("USUARIO_ID").HasMaxLength(36).IsRequired();

            entity.Property(x => x.Proposito).HasColumnName("PROPOSITO").HasMaxLength(30).IsRequired();

            entity.Property(x => x.OtpHash).HasColumnName("OTP_HASH").HasMaxLength(128).IsRequired();
            entity.Property(x => x.OtpSalt).HasColumnName("OTP_SALT").HasMaxLength(64).IsRequired();

            entity.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").IsRequired();
            entity.Property(x => x.FechaExpiracion).HasColumnName("FECHA_EXPIRACION").IsRequired();

            entity.Property(x => x.Usado).HasColumnName("USADO").IsRequired();
            entity.Property(x => x.FechaUso).HasColumnName("FECHA_USO");

            entity.Property(x => x.Intentos).HasColumnName("INTENTOS").IsRequired();
            entity.Property(x => x.MaxIntentos).HasColumnName("MAX_INTENTOS").IsRequired();

            entity.Property(x => x.IpSolicitud).HasColumnName("IP_SOLICITUD").HasMaxLength(45);
            entity.Property(x => x.UserAgent).HasColumnName("USER_AGENT").HasMaxLength(250);

            entity.HasOne(x => x.TaUsuarioModel)
             .WithMany()
             .HasForeignKey(x => x.UsuarioId);

        }
    }
}
