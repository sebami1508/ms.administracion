using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaItemModelEtc : IEntityTypeConfiguration<TaItemModel>
    {
        public void Configure(EntityTypeBuilder<TaItemModel> entity)
        {
            entity.HasKey(e => e.ItemId).HasName("PK_ITEM_ID");

            entity.ToTable("TA_ITEM", "SC_ADMINISTRACION");

            entity.Property(e => e.ItemId)
                .HasColumnName("ITEM_ID")
                .HasMaxLength(40);

            entity.Property(e => e.OrdenId)
                .HasColumnName("ORDEN_ID")
                .HasMaxLength(40);

            entity.Property(e => e.ProductoId)
                .HasColumnName("PRODUCTO_ID")
                .HasMaxLength(40);

            entity.Property(e => e.Cantidad)
                .HasColumnName("CANTIDAD");

            entity.Property(e => e.Subtotal)
                .HasColumnName("SUBTOTAL");

            entity.HasOne(d => d.TaOrdenModel)
                .WithMany(p => p.LtsTaItemModel)
                .HasForeignKey(d => d.OrdenId)
                .HasConstraintName("FK_ORDEN_ID_ITEM");

            entity.HasOne(d => d.TaProductoModel)
                .WithMany(p => p.LtsTaItemModel)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK_PRODUCTO_ID_ITEM");

        }
    }
}
