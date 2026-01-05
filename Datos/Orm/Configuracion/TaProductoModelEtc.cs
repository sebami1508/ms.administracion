using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaProductoModelEtc : IEntityTypeConfiguration<TaProductoModel>
    {
        public void Configure(EntityTypeBuilder<TaProductoModel> builder)
        {
            builder.ToTable("TA_PRODUCTO", "SC_ADMINISTRACION");

            builder.HasKey(s => s.ProductoId);

            builder.Property(s => s.ProductoId)
                   .HasColumnName("PRODUCTO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.CategoriaId)
                   .HasColumnName("CATEGORIA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.Descripcion)
                   .HasColumnName("DESCRIPCION")
                   .HasMaxLength(150);

            builder.Property(s => s.Precio)
                   .HasColumnName("PRECIO");

            builder.Property(s => s.Vigente)
                 .HasColumnName("VIGENTE");

            builder.HasOne(s => s.TaDominioModel)
                    .WithMany(s => s.LtsTaProductoModel)
                    .HasForeignKey(s => s.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
