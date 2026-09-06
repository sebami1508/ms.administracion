using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaUsuarioModelEtc : IEntityTypeConfiguration<TaUsuarioModel>
    {
        public void Configure(EntityTypeBuilder<TaUsuarioModel> builder)
        {

            builder.ToTable("TA_USUARIO", "SC_ADMINISTRACION");

            builder.HasKey(s => s.UsuarioId);

            builder.Property(s => s.UsuarioId)
                   .HasColumnName("USUARIO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Nombres)
                   .HasColumnName("NOMBRES")
                   .HasMaxLength(80);

            builder.Property(s => s.Apellidos)
                   .HasColumnName("APELLIDOS")
                   .HasMaxLength(150);

            builder.Property(s => s.Identificacion)
                   .HasColumnName("IDENTIFICACION");

            builder.Property(s => s.Celular)
				   .HasColumnName("CELULAR")
                   .HasMaxLength(10);

            builder.Property(s => s.CorreoElectronico)
                   .HasColumnName("CORREO_ELECTRONICO")
                   .HasMaxLength(150);

            builder.Property(s => s.Direccion)
                   .HasColumnName("DIRECCION")
                   .HasMaxLength(250);

            builder.Property(s => s.Password)
                  .HasColumnName("PASSWORD")
                  .HasMaxLength(500);

            builder.Property(s => s.IngresoPrimeraVez)
                  .HasColumnName("INGRESO_PRIMERA_VEZ");

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");
        }
    }
}
