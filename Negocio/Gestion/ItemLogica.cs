using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Dto.DtoReader;
using Comun.Dto.DtoUtilidades;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;

namespace Negocio.Gestion
{
    public class ItemLogica : IItem
    {
        private readonly ContextoDb db;
        private readonly CItemValidator validatorC;
        private readonly UItemValidator validatorU;

        public ItemLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new CItemValidator();
            validatorU = new UItemValidator();
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CItemDto;
            var v = await validatorC.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            var newItemId = Guid.NewGuid().ToString();

            var model = new TaItemModel
            {
                ItemId = newItemId,
                OrdenId = dto.OrdenId!.Trim(),
                ProductoId = dto.ProductoId.Trim(),
                Cantidad = dto.Cantidad,
                Subtotal = dto.Subtotal,
                NombrePlato = string.IsNullOrWhiteSpace(dto.NombrePlato)
                                        ? null
                                        : dto.NombrePlato.Trim().ToUpperInvariant()
            };

            db.Add(model);

            if (dto.Caracteristicas != null && dto.Caracteristicas.Any())
            {
                var pizzas = dto.Caracteristicas
                    .Select(c => new TaCaracteristicaModel
                    {
                        CaracteristicaId = Guid.NewGuid().ToString(),
                        ItemId = newItemId,
                        UnSabor = c.UnSabor,
                        EnPatacon = c.EnPatacon,
                        Observacion = c.Observacion
                    });

                await db.AddRangeAsync(pizzas);
            }

            bool ok = await db.SaveChangesAsync() > 0;

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UItemDto;
            var v = await validatorU.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            var model = await db.Set<TaItemModel>().FindAsync(dto!.ItemId);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El item no existe.");

            if (dto.Cantidad.HasValue) model.Cantidad = dto.Cantidad.Value;
            if (dto.Subtotal.HasValue) model.Subtotal = dto.Subtotal.Value;

            db.Update(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            if (_param is not EliminarDto dto || string.IsNullOrWhiteSpace(dto.Id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var item = await db.Set<TaItemModel>().FindAsync(dto.Id);

            if (item is null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El item no existe.");

            await db.Set<TaCaracteristicaModel>()
                .Where(x => x.ItemId == item.ItemId)
                .ExecuteDeleteAsync();

            var orden = await db.Set<TaOrdenModel>().FindAsync(item.OrdenId);

            if (orden is not null)
            {
                orden.Total = Math.Max(0, orden.Total - item.Subtotal);
                orden.CantidadItem = Math.Max(0, orden.CantidadItem - item.Cantidad);
            }

            db.Set<TaItemModel>().Remove(item);

            var cambios = await db.SaveChangesAsync();

            return cambios > 0
                ? new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.")
                : new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.Set<TaItemModel>().Include(i => i.TaProductoModel).Select(i => new RItemDto
            {
                ItemId = i.ItemId,
                OrdenId = i.OrdenId,
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                Subtotal = i.Subtotal,
                ProductoDescripcion = i.TaProductoModel.Descripcion
            }).OrderBy(o => o.ItemId).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RItemDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }
    }
}
