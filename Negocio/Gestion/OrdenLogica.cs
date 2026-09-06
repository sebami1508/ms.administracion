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

namespace Negocio.Gestion
{

    public class OrdenLogica : IOrden
    {
        #region Atributos
        private readonly ContextoDb db;
        private readonly COrdenValidator validatorC;
        private readonly UOrdenValidator validatorU;
        private readonly IFacturacionService _facturacion;
        private readonly IFcmService _fcm;
        private readonly IRealtimeNotificador _realtime;
        #endregion

        #region Constructor
        public OrdenLogica(ContextoDb _db, IFacturacionService facturacion, IFcmService fcm, IRealtimeNotificador realtime)
        {
            db = _db;
            validatorC = new COrdenValidator();
            validatorU = new UOrdenValidator();
            _facturacion = facturacion ?? throw new ArgumentNullException(nameof(facturacion));
            _fcm = fcm ?? throw new ArgumentNullException(nameof(fcm));
            _realtime = realtime ?? throw new ArgumentNullException(nameof(realtime));
        }
        #endregion

        #region Métodos

        static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        static string? Upper(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant();

        static object ConstruirPayload(TaOrdenModel m) => new
        {
            ordenId = m.OrdenId,
            codigo = m.Codigo,
            estadoId = m.EstadoId,
            mesa = m.Mesa,
            cliente = m.Cliente,
            usuarioId = m.UsuarioId,
            total = m.Total
        };

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
                    UsuarioId = dto.UsuarioId!.Trim(),
                    Total = dto.Total,
                    EstadoId = string.IsNullOrWhiteSpace(dto.EstadoId)
                        ? Constantes.Pendiente
                        : dto.EstadoId.Trim(),
                    Mesa = dto.Mesa,
                    Codigo = dto.Codigo,
                    Vigente = true,
                    FechaRegistro = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified),
                    Domicilio = dto.Domicilio,
                    Cliente = dto.Cliente != null ? dto.Cliente?.Trim().ToUpperInvariant() : null,
                    Direccion = dto.Direccion,
                    TurnoId = dto.TurnoId
                };

                await db.AddAsync(model);

                var items = new List<TaItemModel>();
                var pizzas = new List<TaCaracteristicaModel>();

                foreach (var p in dto.Productos)
                {
                    var newItemId = Guid.NewGuid().ToString();

                    var item = new TaItemModel
                    {
                        ItemId = newItemId,
                        OrdenId = ordenId,
                        ProductoId = p.ProductoId,
                        Cantidad = p.Cantidad,
                        Subtotal = p.Subtotal,
                        NombrePlato = Upper(p.NombrePlato),
                        Observacion = Clean(p.Observacion)
                    };


                    items.Add(item);

                    if (p.Caracteristicas != null && p.Caracteristicas.Any())
                    {
                        foreach (var c in p.Caracteristicas)
                        {
                            pizzas.Add(new TaCaracteristicaModel
                            {
                                CaracteristicaId = Guid.NewGuid().ToString(),
                                ItemId = newItemId,
                                UnSabor = c.UnSabor,
                                EnPatacon = c.EnPatacon,
                                Observacion = c.Observacion
                            });
                        }
                    }
                }

                await db.AddRangeAsync(items);

                if (pizzas.Count > 0)
                    await db.AddRangeAsync(pizzas);

                bool guardado = await db.SaveChangesAsync() > 0;

                if (!guardado)
                {
                    await transaction.RollbackAsync();
                    return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se logró guardar los datos de la orden.");
                }

                await transaction.CommitAsync();

                // Tiempo real: solo avisa al personal cuando la orden entra "Por validar"
                // (creada desde la App del cliente). Las que crea el personal no aplican.
                if (model.EstadoId == Constantes.PorValidar)
                    await _realtime.OrdenNuevaAsync(ConstruirPayload(model));

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

            model.CantidadItem = dto.CantidadItem ?? model.CantidadItem;
            model.Total = dto.Total ?? model.Total;
            model.TotalEfectivo = dto.TotalEfectivo ?? model.TotalEfectivo;
            model.TotalTransferencia = dto.TotalTransferencia ?? model.TotalTransferencia;

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

            new GestionLogica(db).ActualizarCamposAutomatico(dto, model);

            var ok = await db.SaveChangesAsync() > 0;

            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");

            // Tiempo real: avisa al personal y al cliente dueño del cambio de estado.
            await _realtime.OrdenEstadoCambiadoAsync(model.UsuarioId, ConstruirPayload(model));

            if (!string.IsNullOrWhiteSpace(numeroFacturaGenerado))
                return new RespuestaDto<TReturn>(
                    EstadoOperacion.Bueno,
                    "Factura generada correctamente.",
                    (TReturn)(object)numeroFacturaGenerado
                );

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
        }



        public async Task<RespuestaDto<TReturn>> AceptarOrdenAsync<TParam, TReturn>(TParam _param)
        {
            var ordenId = _param as string;
            if (string.IsNullOrWhiteSpace(ordenId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador de orden inválido.");

            var model = await db.Set<TaOrdenModel>().FindAsync(ordenId.Trim());
            if (model is null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La orden no existe.");

            if (model.EstadoId != Constantes.PorValidar)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "La orden no está en estado 'Por validar'.");

            model.EstadoId = Constantes.Pendiente;
            db.Update(model);

            var ok = await db.SaveChangesAsync() > 0;
            if (!ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No se pudo aceptar la orden.");

            // Notificar al cliente dueño de la orden (push). No debe romper el flujo.
            await _fcm.EnviarAsync(
                model.UsuarioId,
                "¡Orden aceptada!",
                "Tu orden fue aceptada y entró en preparación.",
                new Dictionary<string, string>
                {
                    { "ordenId", model.OrdenId },
                    { "estadoId", Constantes.Pendiente },
                    { "tipo", "ORDEN_ACEPTADA" }
                });

            // Tiempo real: el personal la saca de "Por validar" y el cliente ve el cambio.
            await _realtime.OrdenEstadoCambiadoAsync(model.UsuarioId, ConstruirPayload(model));

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Orden aceptada correctamente.");
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
                    TotalEfectivo = o.TotalEfectivo,
                    TotalTransferencia = o.TotalTransferencia,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    UsuarioCelular = o.TaUsuarioModel.Celular,
                    UsuarioCorreo = o.TaUsuarioModel.CorreoElectronico,
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
                            NombrePlato = i.NombrePlato,
                            Observacion = i.Observacion,
                            Caracteristicas = i.LtsTaCaracteristicaModel
                                .Select(pz => new RCaracteristicaDto
                                {
                                    CaracteristicaId = pz.CaracteristicaId,
                                    UnSabor = pz.UnSabor,
                                    EnPatacon = pz.EnPatacon,
                                    Observacion = pz.Observacion
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
                    TotalEfectivo = o.TotalEfectivo,
                    TotalTransferencia = o.TotalTransferencia,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    UsuarioCelular = o.TaUsuarioModel.Celular,
                    UsuarioCorreo = o.TaUsuarioModel.CorreoElectronico,
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
                            NombrePlato = i.NombrePlato,
                            Observacion = i.Observacion,
                            Caracteristicas = i.LtsTaCaracteristicaModel
                                .Select(pz => new RCaracteristicaDto
                                {
                                    CaracteristicaId = pz.CaracteristicaId,
                                    UnSabor = pz.UnSabor,
                                    EnPatacon = pz.EnPatacon,
                                    Observacion = pz.Observacion
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
            var ahora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
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
                    TotalEfectivo = o.TotalEfectivo,
                    TotalTransferencia = o.TotalTransferencia,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    UsuarioCelular = o.TaUsuarioModel.Celular,
                    UsuarioCorreo = o.TaUsuarioModel.CorreoElectronico,
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
                            NombrePlato = i.NombrePlato,
                            Observacion = i.Observacion,
                            Caracteristicas = i.LtsTaCaracteristicaModel
                                .Select(pz => new RCaracteristicaDto
                                {
                                    CaracteristicaId = pz.CaracteristicaId,
                                    UnSabor = pz.UnSabor,
                                    EnPatacon = pz.EnPatacon,
                                    Observacion = pz.Observacion
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


        public async Task<RespuestaDto<TReturn>> ConsultarListaOrdenesPorTurnoIdAsync<TParam, TReturn>(TParam _param)
        {
            string turnoId = _param as string;

            if (string.IsNullOrWhiteSpace(turnoId))
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "El identificador es necesario.");

            var resultados = await db.Set<TaOrdenModel>()
                .AsNoTracking()
                .Where(o => o.TurnoId == turnoId)
                .OrderByDescending(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    TotalEfectivo = o.TotalEfectivo,
                    TotalTransferencia = o.TotalTransferencia,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    UsuarioCelular = o.TaUsuarioModel.Celular,
                    UsuarioCorreo = o.TaUsuarioModel.CorreoElectronico,
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
                            NombrePlato = i.NombrePlato,
                            Observacion = i.Observacion,
                            Caracteristicas = i.LtsTaCaracteristicaModel
                                .Select(pz => new RCaracteristicaDto
                                {
                                    CaracteristicaId = pz.CaracteristicaId,
                                    UnSabor = pz.UnSabor,
                                    EnPatacon = pz.EnPatacon,
                                    Observacion = pz.Observacion
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

            static DateTime Unspec(DateTime dt) =>
                DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);

            var fechaInicio = Unspec(dto.FechaInicio.Date);
            var fechaFinExclusiva = Unspec(dto.FechaFin.Date.AddDays(1));

            query = query.Where(o => o.FechaRegistro >= fechaInicio && o.FechaRegistro < fechaFinExclusiva);

            var resultados = await query
                .Where(x => x.EstadoId == Constantes.Facturada)
                .OrderBy(o => o.FechaRegistro)
                .Select(o => new ROrdenDto
                {
                    OrdenId = o.OrdenId,
                    CantidadItem = o.CantidadItem,
                    Total = o.Total,
                    TotalEfectivo = o.TotalEfectivo,
                    TotalTransferencia = o.TotalTransferencia,
                    UsuarioId = o.UsuarioId,
                    UsuarioIdStr = $"{o.TaUsuarioModel.Nombres} {o.TaUsuarioModel.Apellidos}",
                    UsuarioCelular = o.TaUsuarioModel.Celular,
                    UsuarioCorreo = o.TaUsuarioModel.CorreoElectronico,
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
                            NombrePlato = i.NombrePlato,
                            Observacion = i.Observacion,
                            Caracteristicas = i.LtsTaCaracteristicaModel
                                .Select(pz => new RCaracteristicaDto
                                {
                                    CaracteristicaId = pz.CaracteristicaId,
                                    UnSabor = pz.UnSabor,
                                    EnPatacon = pz.EnPatacon,
                                    Observacion = pz.Observacion
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
        #endregion
    }
}
