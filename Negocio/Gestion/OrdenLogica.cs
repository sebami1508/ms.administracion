using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Dto.DtoReader;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Negocio.Gestion
{
    public class OrdenLogica : IOrden
    {
        private readonly ContextoDb db;
        private readonly COrdenValidator validatorC;
        private readonly UOrdenValidator validatorU;

        public OrdenLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new COrdenValidator();
            validatorU = new UOrdenValidator();
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam param)
        {
            if (param is not COrdenDto dto)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Los parámetros de entrada no corresponden a una orden.");

            var v = await validatorC.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            if (dto.Productos == null || !dto.Productos.Any())
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La orden debe tener al menos un producto.");

            var transaction = await db.Database.BeginTransactionAsync();

            try
            {

                var model = new TaOrdenModel
                {
                    OrdenId = dto.OrdenId,
                    CantidadItem = dto.CantidadItem,
                    Total = dto.Total,
                    UsuarioId = dto.UsuarioId?.Trim(),
                    EstadoId = Constantes.Pendiente,
                    Mesa = dto.Mesa,
                    Codigo = GenerarCodigoOrden(),
                    Vigente = true,
                    FechaRegistro = DateTime.UtcNow
                };

                await db.AddAsync(model);

                var items = dto.Productos.Select(p => new TaItemModel
                {
                    ItemId = Guid.NewGuid().ToString(),
                    OrdenId = p.OrdenId,
                    ProductoId = p.ProductoId,
                    Cantidad = p.Cantidad,
                    Subtotal = p.Subtotal
                }).ToList();

                await db.AddRangeAsync(items);

                var auditoria = new GestionAuditoriaLogica(db);
                bool guardado = await auditoria.SaveChangesAsync(dto.ParametrosAuditoriaDto);

                if (!guardado)
                {
                    await transaction.RollbackAsync();
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se logró guardar los datos de la orden.");
                }

                await transaction.CommitAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ocurrió un error al guardar la orden.");
            }
        }

        private string GenerarCodigoOrden()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var buffer = new byte[6];

            RandomNumberGenerator.Fill(buffer);

            var sb = new StringBuilder(10);
            sb.Append("BRA-");

            foreach (var b in buffer)
                sb.Append(chars[b % chars.Length]);

            return sb.ToString();
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam param)
        {
            if (param is not UOrdenDto dto)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Los datos de la orden no son válidos.");

            var v = await validatorU.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            var model = await db.Set<TaOrdenModel>().FindAsync(dto.OrdenId);
            if (model is null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La orden no existe.");

            if (model.FechaRegistro.Kind != DateTimeKind.Utc)
                model.FechaRegistro = model.FechaRegistro.ToUniversalTime();

            model.CantidadItem = dto.CantidadItem ?? model.CantidadItem;
            model.Total = dto.Total ?? model.Total;

            if (!string.IsNullOrWhiteSpace(dto.EstadoId))
                model.EstadoId = dto.EstadoId.Trim();

            if (!string.IsNullOrWhiteSpace(dto.MetodoPagoId))
                model.MetodoPagoId = dto.MetodoPagoId.Trim();

            var auditoria = new GestionAuditoriaLogica(db);
            auditoria.ActualizarCamposAutomatico(dto, model);
            var ok = await auditoria.SaveChangesAsync(dto.ParametrosAuditoriaDto);

            return ok
                ? new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.")
                : new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }


        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var id = _param as string;
            if (string.IsNullOrWhiteSpace(id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.Set<TaOrdenModel>().FindAsync(id);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La orden no existe.");

            model.Vigente = false;
            db.Update(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.Set<TaOrdenModel>()
                .AsNoTracking()
                .OrderByDescending(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    EstadoId = o.EstadoId,
                    EstadoIdStr = o.TaDominioModel.Descripcion,
                    FechaRegistro = o.FechaRegistro,
                    Mesa = o.Mesa,
                    Codigo = o.Codigo,
                    Productos = o.LtsTaItemModel
                        .OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal
                        })
                        .ToList()
                })
                .ToListAsync();

            if (!resultados.Any())
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se encontraron órdenes.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)(object)resultados);
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaPorEstadoIdAsync<TParam, TReturn>(TParam _param)
        {
            var estadoId = _param as string;

            if (string.IsNullOrWhiteSpace(estadoId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El identificador de estado es inválido.");

            var resultados = await db.Set<TaOrdenModel>()
                .AsNoTracking()
                .Where(o => o.EstadoId == estadoId)
                .OrderByDescending(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    EstadoId = o.EstadoId,
                    EstadoIdStr = o.TaDominioModel.Descripcion,
                    Codigo = o.Codigo,
                    FechaRegistro = o.FechaRegistro,
                    Mesa = o.Mesa,
                    Productos = o.LtsTaItemModel
                    .OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal
                        })
                        .ToList()
                })
                .ToListAsync();

            if (!resultados.Any())
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se encontraron órdenes.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)(object)resultados);
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaOrdenesDelDiaAsync<TReturn>()
        {
            var ahora = DateTime.UtcNow;
            var inicio = ahora.Date;
            var fin = inicio.AddDays(1).AddHours(2);

            var resultados = await db.Set<TaOrdenModel>()
                .AsNoTracking()
                .Where(o => o.FechaRegistro >= inicio
                            && o.FechaRegistro <= fin)
                .OrderByDescending(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    EstadoId = o.EstadoId,
                    EstadoIdStr = o.TaDominioModel.Descripcion,
                    Codigo = o.Codigo,
                    FechaRegistro = o.FechaRegistro,
                    Mesa = o.Mesa,
                    Productos = o.LtsTaItemModel
                    .OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal
                        })
                        .ToList()
                })
                .ToListAsync();

            if (!resultados.Any())
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se encontraron órdenes.");

            return new RespuestaDto<TReturn>(
                EstadoOperacion.Bueno,
                "Operación exitosa",
                (TReturn)(object)resultados
            );
        }


        public async Task<RespuestaDto<TReturn>> ConsultarListaOrdenesRangoDeFechasAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as PFiltroOrdenesDto;
            var query = db.TaOrdenModel.AsQueryable().AsNoTracking();

            DateTime fechaInicio = dto.FechaInicio.Date;
            DateTime fechaFin = new DateTime(dto.FechaFin.Year, dto.FechaFin.Month, dto.FechaFin.Day, 23, 59, 59);
            query = query.Where(i => i.FechaRegistro >= fechaInicio && i.FechaRegistro <= fechaFin);

            var resultados = await query
                .OrderBy(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    EstadoId = o.EstadoId,
                    EstadoIdStr = o.TaDominioModel.Descripcion,
                    Codigo = o.Codigo,
                    FechaRegistro = o.FechaRegistro,
                    Mesa = o.Mesa,
                    Productos = o.LtsTaItemModel.OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal
                        })
                        .ToList()
                })
                .ToListAsync();

            if (!resultados.Any())
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se encontraron órdenes.");

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)(object)resultados);
        }

    }
}
