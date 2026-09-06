using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaOtpRegistroModelEtc : IEntityTypeConfiguration<TaOtpRegistroModel>
    {
        public void Configure(EntityTypeBuilder<TaOtpRegistroModel> entity)
        {
            entity.ToTable("TA_OTP_REGISTRO", "SC_ADMINISTRACION");

            entity.HasKey(x => x.OtpRegistroId);

            entity.Property(x => x.OtpRegistroId).HasColumnName("OTP_REGISTRO_ID").IsRequired();
            entity.Property(x => x.Correo).HasColumnName("CORREO").HasMaxLength(150).IsRequired();

            entity.Property(x => x.OtpHash).HasColumnName("OTP_HASH").HasMaxLength(128).IsRequired();
            entity.Property(x => x.OtpSalt).HasColumnName("OTP_SALT").HasMaxLength(64).IsRequired();

            entity.Property(x => x.FechaCreacion).HasColumnName("FECHA_CREACION").IsRequired();
            entity.Property(x => x.FechaExpiracion).HasColumnName("FECHA_EXPIRACION").IsRequired();

            entity.Property(x => x.Usado).HasColumnName("USADO").IsRequired();
            entity.Property(x => x.FechaUso).HasColumnName("FECHA_USO");

            entity.Property(x => x.Intentos).HasColumnName("INTENTOS").IsRequired();
            entity.Property(x => x.MaxIntentos).HasColumnName("MAX_INTENTOS").IsRequired();
        }
    }
}
