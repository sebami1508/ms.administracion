using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaPizzaModelEtc : IEntityTypeConfiguration<TaPizzaModel>
    {
        public void Configure(EntityTypeBuilder<TaPizzaModel> builder)
        {
            builder.ToTable("TA_PIZZA", "SC_ADMINISTRACION");

            builder.HasKey(s => s.PizzaId);

            builder.Property(s => s.PizzaId)
                   .HasColumnName("PIZZA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.ItemId)
                   .HasColumnName("ITEM_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.TipoId)
                   .HasColumnName("TIPO_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.SaborId)
                   .HasColumnName("SABOR_ID")
                   .HasMaxLength(40);

            builder.HasOne(s => s.TaItemModel)
                    .WithMany(s => s.LtsTaPizzaModel)
                    .HasForeignKey(s => s.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModelTipo)
                    .WithMany(s => s.LtsTaPizzaModelTipo)
                    .HasForeignKey(s => s.TipoId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(s => s.TaDominioModelSabor)
                    .WithMany(s => s.LtsTaPizzaModelSabor)
                    .HasForeignKey(s => s.SaborId)
                    .OnDelete(DeleteBehavior.Restrict);

        }
    }

}
