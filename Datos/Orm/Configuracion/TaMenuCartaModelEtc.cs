using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaMenuCartaModelEtc : IEntityTypeConfiguration<TaMenuCartaModel>
    {
        public void Configure(EntityTypeBuilder<TaMenuCartaModel> builder)
        {
            builder.ToTable("TA_MENU_CARTA", "SC_ADMINISTRACION");

            builder.HasKey(s => s.MenuCartaId);

            builder.Property(s => s.MenuCartaId)
                   .HasColumnName("MENU_CARTA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.NombreArchivo)
                   .HasColumnName("NOMBRE_ARCHIVO")
                   .HasMaxLength(200);

            builder.Property(s => s.Contenido)
                   .HasColumnName("CONTENIDO")
                   .HasColumnType("text");

            builder.Property(s => s.FechaRegistro)
                   .HasColumnName("FECHA_REGISTRO")
                   .HasColumnType("timestamp without time zone");

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");
        }
    }
}
