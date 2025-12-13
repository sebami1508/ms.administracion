using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaPersonaModelEtc : IEntityTypeConfiguration<TaPersonaModel>
    {
        public void Configure(EntityTypeBuilder<TaPersonaModel> builder)
        {
            builder.ToTable("TA_PERSONA", "SC_ADMINISTRACION");

            builder.HasKey(s => s.PersonaId);

            builder.Property(s => s.PersonaId)
                   .HasColumnName("PERSONA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Nombres)
                   .HasColumnName("NOMBRES")
                   .HasMaxLength(80);

            builder.Property(s => s.Apellidos)
                   .HasColumnName("APELLIDOS")
                   .HasMaxLength(150);

            builder.Property(s => s.TipoDocumentoId)
                   .HasColumnName("TIPO_DOCUMENTO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.NumeroIdentificacion)
                   .HasColumnName("NUMERO_IDENTIFICACION");


            builder.Property(s => s.FechaExpedicion)
                   .HasColumnName("FECHA_EXPEDICION");

            builder.Property(s => s.Correo)
                   .HasColumnName("CORREO")
                   .HasMaxLength(150);

            builder.Property(s => s.Telefono)
                   .HasColumnName("TELEFONO")
                   .HasMaxLength(15);

            builder.Property(s => s.Direccion)
                   .HasColumnName("DIRECCION")
                   .HasMaxLength(200);

            builder.Property(s => s.FechaNacimiento)
                   .HasColumnName("FECHA_NACIMIENTO");

            builder.Property(s => s.GeneroId)
                 .HasColumnName("GENERO_ID")
                 .HasMaxLength(40);

            builder.Property(s => s.TerminosCondiciones)
                   .HasColumnName("TERMINOS_Y_CONDICIONES");

            builder.Property(s => s.PoliticaTratamientoDatos)
                   .HasColumnName("POLITICA_TRATAMIENTO_DATOS");

            builder.Property(s => s.MayorEdad)
                   .HasColumnName("MAYOR_EDAD");

            builder.Property(s => s.ResponsabilidadFiscalId)
                 .HasColumnName("RESPONSABILIDAD_FISCAL_ID")
                 .HasMaxLength(40);

            builder.HasOne(s => s.TaDominioModelTipoDocumento)
              .WithMany(s => s.LtsTaPersonaModelTipoDocumento)
              .HasForeignKey(s => s.TipoDocumentoId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModelGenero)
              .WithMany(s => s.LtsTaPersonaModelGenero)
              .HasForeignKey(s => s.GeneroId)
              .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModelResponsabilidad)
              .WithMany(s => s.LtsTaPersonaModelResponsabilidad)
              .HasForeignKey(s => s.ResponsabilidadFiscalId)
              .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
