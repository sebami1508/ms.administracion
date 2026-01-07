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
        private readonly IFacturacionService _facturacion;

        public OrdenLogica(ContextoDb _db, IFacturacionService facturacion)
        {
            db = _db;
            validatorC = new COrdenValidator();
            validatorU = new UOrdenValidator();
            _facturacion = facturacion ?? throw new ArgumentNullException(nameof(facturacion));
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
                var ordenId = Guid.NewGuid().ToString();

                var model = new TaOrdenModel
                {
                    OrdenId = ordenId,
                    CantidadItem = dto.CantidadItem,
                    Total = dto.Total,
                    UsuarioId = dto.UsuarioId?.Trim(),
                    EstadoId = Constantes.Pendiente,
                    Mesa = dto.Mesa,
                    Codigo = dto.Codigo,
                    Vigente = true,
                    FechaRegistro = DateTime.UtcNow,
                    Domicilio = dto.Domicilio,
                    Cliente = dto.Cliente,
                    Direccion = dto.Direccion
                };

                await db.AddAsync(model);

                var items = new List<TaItemModel>();
                var pizzas = new List<TaPizzaModel>();

                foreach (var p in dto.Productos)
                {
                    var newItemId = Guid.NewGuid().ToString();

                    var item = new TaItemModel
                    {
                        ItemId = newItemId,
                        OrdenId = ordenId,
                        ProductoId = p.ProductoId,
                        Cantidad = p.Cantidad,
                        Subtotal = p.Subtotal
                    };

                    items.Add(item);

                    if (p.Caracteristicas != null && p.Caracteristicas.Any())
                    {
                        foreach (var c in p.Caracteristicas)
                        {
                            pizzas.Add(new TaPizzaModel
                            {
                                PizzaId = Guid.NewGuid().ToString(),
                                ItemId = newItemId,
                                TipoId = c.TipoId,
                                SaborId = c.SaborId
                            });
                        }
                    }
                }

                await db.AddRangeAsync(items);

                if (pizzas.Count > 0)
                    await db.AddRangeAsync(pizzas);

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

            string? numeroFacturaGenerado = null;

            if (!string.IsNullOrWhiteSpace(dto.MetodoPagoId))
            {
                model.MetodoPagoId = dto.MetodoPagoId.Trim();

                if (string.IsNullOrWhiteSpace(model.NumeroFactura))
                {
                    numeroFacturaGenerado = await _facturacion.GenerarNumeroFacturaAsync();
                    model.NumeroFactura = numeroFacturaGenerado;
                }
            }

            var auditoria = new GestionAuditoriaLogica(db);
            auditoria.ActualizarCamposAutomatico(dto, model);

            var ok = await auditoria.SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");

            if (!string.IsNullOrWhiteSpace(numeroFacturaGenerado))
                return new RespuestaDto<TReturn>(
                    EstadoOperacion.Bueno,
                    "Factura generada correctamente.",
                    (TReturn)(object)numeroFacturaGenerado
                );

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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
                    Codigo = o.Codigo,
                    FechaRegistro = o.FechaRegistro,
                    Mesa = o.Mesa,
                    Domicilio = o.Domicilio,
                    Cliente = o.Cliente,
                    Direccion = o.Direccion,
                    MetodoPagoId = o.MetodoPagoId,
                    MetodoPagoIdStr = o.TaDominioModel2.Descripcion,
                    NumeroFactura = o.NumeroFactura,
                    Productos = o.LtsTaItemModel.OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            OrdenId = i.OrdenId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            CategoriaId = i.TaProductoModel.CategoriaId,
                            CategoriaIdStr = i.TaProductoModel.TaDominioModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal,
                            Caracteristicas = i.LtsTaPizzaModel
                                .Select(pz => new RPizzaDto
                                {
                                    PizzaId = pz.PizzaId,
                                    ItemId = pz.ItemId,
                                    TipoId = pz.TipoId,
                                    TipoIdStr = pz.TaDominioModelTipo.Descripcion,
                                    SaborId = pz.SaborId,
                                    SaborIdStr = pz.TaDominioModelSabor.Descripcion
                                })
                                .ToList()
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
                    Domicilio = o.Domicilio,
                    Cliente = o.Cliente,
                    Direccion = o.Direccion,
                    MetodoPagoId = o.MetodoPagoId,
                    MetodoPagoIdStr = o.TaDominioModel2.Descripcion,
                    NumeroFactura = o.NumeroFactura,
                    Productos = o.LtsTaItemModel.OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            OrdenId = i.OrdenId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            CategoriaId = i.TaProductoModel.CategoriaId,
                            CategoriaIdStr = i.TaProductoModel.TaDominioModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal,
                            Caracteristicas = i.LtsTaPizzaModel
                                .Select(pz => new RPizzaDto
                                {
                                    PizzaId = pz.PizzaId,
                                    ItemId = pz.ItemId,
                                    TipoId = pz.TipoId,
                                    TipoIdStr = pz.TaDominioModelTipo.Descripcion,
                                    SaborId = pz.SaborId,
                                    SaborIdStr = pz.TaDominioModelSabor.Descripcion
                                })
                                .ToList()
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
                    Domicilio = o.Domicilio,
                    Cliente = o.Cliente,
                    Direccion = o.Direccion,
                    MetodoPagoId = o.MetodoPagoId,
                    MetodoPagoIdStr = o.TaDominioModel2.Descripcion,
                    NumeroFactura = o.NumeroFactura,
                    Productos = o.LtsTaItemModel.OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            OrdenId = i.OrdenId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            CategoriaId = i.TaProductoModel.CategoriaId,
                            CategoriaIdStr = i.TaProductoModel.TaDominioModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal,
                            Caracteristicas = i.LtsTaPizzaModel
                                .Select(pz => new RPizzaDto
                                {
                                    PizzaId = pz.PizzaId,
                                    ItemId = pz.ItemId,
                                    TipoId = pz.TipoId,
                                    TipoIdStr = pz.TaDominioModelTipo.Descripcion,
                                    SaborId = pz.SaborId,
                                    SaborIdStr = pz.TaDominioModelSabor.Descripcion
                                })
                                .ToList()
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

            DateTime fechaInicio = DateTime.SpecifyKind(dto.FechaInicio.Date, DateTimeKind.Utc);
            DateTime fechaFin = new DateTime(dto.FechaFin.Year, dto.FechaFin.Month, dto.FechaFin.Day, 23, 59, 59, DateTimeKind.Utc);
            query = query.Where(i => i.FechaRegistro >= fechaInicio && i.FechaRegistro <= fechaFin);

            var resultados = await query
                .Where(x => x.EstadoId == Constantes.Facturada)
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
                    Domicilio = o.Domicilio,
                    Cliente = o.Cliente,
                    Direccion = o.Direccion,
                    MetodoPagoId = o.MetodoPagoId,
                    MetodoPagoIdStr = o.TaDominioModel2.Descripcion,
                    NumeroFactura = o.NumeroFactura,
                    Productos = o.LtsTaItemModel.OrderBy(i => i.TaProductoModel.TaDominioModel.Descripcion)
                        .Select(i => new RItemDto
                        {
                            ItemId = i.ItemId,
                            OrdenId = i.OrdenId,
                            ProductoId = i.ProductoId,
                            ProductoDescripcion = i.TaProductoModel.Descripcion,
                            CategoriaId = i.TaProductoModel.CategoriaId,
                            CategoriaIdStr = i.TaProductoModel.TaDominioModel.Descripcion,
                            Cantidad = i.Cantidad,
                            Subtotal = i.Subtotal,
                            Caracteristicas = i.LtsTaPizzaModel
                                .Select(pz => new RPizzaDto
                                {
                                    PizzaId = pz.PizzaId,
                                    ItemId = pz.ItemId,
                                    TipoId = pz.TipoId,
                                    TipoIdStr = pz.TaDominioModelTipo.Descripcion,
                                    SaborId = pz.SaborId,
                                    SaborIdStr = pz.TaDominioModelSabor.Descripcion
                                })
                                .ToList()
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
