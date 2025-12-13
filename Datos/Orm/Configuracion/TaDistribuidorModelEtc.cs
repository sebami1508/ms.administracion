using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Orm.Configuracion
{
    public class TaDistribuidorModelEtc : IEntityTypeConfiguration<TaDistribuidorModel>
    {
        public void Configure(EntityTypeBuilder<TaDistribuidorModel> builder)
        {

            builder.ToTable("TA_DISTRIBUIDOR", "SC_ADMINISTRACION");

            builder.HasKey(s => s.DistribuidorId);

            builder.Property(s => s.DistribuidorId)
                   .HasColumnName("DISTRIBUIDOR_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Nombre)
                   .HasColumnName("NOMBRE")
                   .HasMaxLength(150);

            builder.Property(s => s.TipoIdentificacionId)
                 .HasColumnName("TIPO_IDENTIFICACION_ID")
                 .HasMaxLength(40);

            builder.Property(s => s.NumeroIdentificacion)
                 .HasColumnName("NUMERO_IDENTIFICACION");

            builder.Property(s => s.Direccion)
                   .HasColumnName("DIRECCION")
                   .HasMaxLength(150);

            builder.Property(s => s.PersonaContacto)
                   .HasColumnName("PERSONA_CONTACTO")
                   .HasMaxLength(150);

            builder.Property(s => s.Telefono)
                   .HasColumnName("TELEFONO")
                   .HasMaxLength(15);

            builder.Property(s => s.Correo)
                   .HasColumnName("CORREO")
                   .HasMaxLength(150);

            builder.Property(s => s.Vigente)
                  .HasColumnName("VIGENTE");

            builder.HasOne(s => s.TaDominioModel)
                   .WithMany(s => s.LtsTaDistribuidorModel)
                   .HasForeignKey(s => s.TipoIdentificacionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
