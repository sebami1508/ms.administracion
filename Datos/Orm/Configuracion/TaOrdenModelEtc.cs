using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Datos.Orm.Configuracion
{
    public class TaOrdenModelEtc : IEntityTypeConfiguration<TaOrdenModel>
    {
        public void Configure(EntityTypeBuilder<TaOrdenModel> entity)
        {
            entity.HasKey(e => e.OrdenId).HasName("PK_ORDEN_ID");

            entity.ToTable("TA_ORDEN", "SC_ADMINISTRACION");

            entity.Property(e => e.OrdenId)
                .HasColumnName("ORDEN_ID")
                .HasMaxLength(40);

            entity.Property(e => e.CantidadItem)
                .HasColumnName("CANTIDAD_ITEM");

            entity.Property(e => e.Total)
                .HasColumnName("TOTAL");

            entity.Property(e => e.UsuarioId)
                .HasColumnName("USUARIO_ID")
                .HasMaxLength(40);

            entity.Property(e => e.Vigente)
                .HasColumnName("VIGENTE");

            entity.Property(e => e.EstadoId)
               .HasColumnName("ESTADO_ID")
               .HasMaxLength(40);

            entity.Property(e => e.FechaRegistro)
                .HasColumnName("FECHA_REGISTRO");

            entity.Property(e => e.Mesa)
                .HasColumnName("MESA");

            entity.Property(e => e.Codigo)
               .HasColumnName("CODIGO")
               .HasMaxLength(10);

            entity.Property(e => e.MetodoPagoId)
               .HasColumnName("METODO_PAGO")
               .HasMaxLength(40);

            entity.Property(e => e.Cliente)
             .HasColumnName("CLIENTE")
             .HasMaxLength(80);

            entity.Property(e => e.Direccion)
             .HasColumnName("DIRECCION")
             .HasMaxLength(100);

            entity.Property(e => e.Domicilio)
                .HasColumnName("DOMICILIO");

            entity.HasOne(d => d.TaUsuarioModel)
                .WithMany(p => p.LtsTaOrdenModel)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_USUARIO_ID_ORDEN");

            entity.HasOne(d => d.TaDominioModel)
                .WithMany(p => p.LtsTaOrdenModel)
                .HasForeignKey(d => d.EstadoId)
                .HasConstraintName("FK_ESTADO_ID_ORDEN");

            entity.HasOne(d => d.TaDominioModel2)
             .WithMany(p => p.LtsTaOrdenModel2)
             .HasForeignKey(d => d.MetodoPagoId)
             .HasConstraintName("FK_METODO_PAGO_ORDEN");

        }
    }
}
