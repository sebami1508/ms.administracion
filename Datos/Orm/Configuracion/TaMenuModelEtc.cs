using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaMenuModelEtc : IEntityTypeConfiguration<TaMenuModel>
    {
        public void Configure(EntityTypeBuilder<TaMenuModel> builder)
        {

            builder.ToTable("TA_MENU", "SC_ADMINISTRACION");

            builder.HasKey(s => s.MenuId);

            builder.Property(s => s.MenuId)
                   .HasColumnName("MENU_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Nombre)
                 .HasColumnName("NOMBRE")
                 .HasMaxLength(50);

            builder.Property(s => s.Icono)
                 .HasColumnName("ICONO")
                 .HasMaxLength(50);

            builder.Property(s => s.Ruta)
                   .HasColumnName("RUTA")
                   .HasMaxLength(50);

            builder.Property(s => s.SubMenu)
                   .HasColumnName("SUB_MENU");

            builder.Property(s => s.MenuPadre)
                   .HasColumnName("MENU_PADRE")
                   .HasMaxLength(40);

            builder.Property(s => s.Orden)
                   .HasColumnName("ORDEN");

            builder.Property(s => s.Vigente)
                   .HasColumnName("VIGENTE");
        }
    }
}
