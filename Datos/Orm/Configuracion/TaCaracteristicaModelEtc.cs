using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Datos.Orm.Configuracion
{
    public class TaCaracteristicaModelEtc : IEntityTypeConfiguration<TaCaracteristicaModel>
    {
        public void Configure(EntityTypeBuilder<TaCaracteristicaModel> builder)
        {
            builder.ToTable("TA_CARACTERISTICA", "SC_ADMINISTRACION");

            builder.HasKey(s => s.CaracteristicaId);

            builder.Property(s => s.CaracteristicaId)
                   .HasColumnName("CARACTERISTICA_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.ItemId)
                   .HasColumnName("ITEM_ID")
                   .HasMaxLength(40);

            builder.Property(s => s.UnSabor)
                   .HasColumnName("UN_SABOR");

            builder.Property(s => s.EnPatacon)
                   .HasColumnName("EN_PATACON");

            builder.Property(s => s.Observacion)
                 .HasColumnName("OBSERVACION")
                 .HasMaxLength(150);

            builder.HasOne(s => s.TaItemModel)
                    .WithMany(s => s.LtsTaCaracteristicaModel)
                    .HasForeignKey(s => s.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
