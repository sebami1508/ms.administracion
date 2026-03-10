using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using ClosedXML.Excel;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Validador;
using System.Globalization;
using System.Text.RegularExpressions;
using Comun.Dto.DtoUtilidades;

namespace Negocio.Gestion
{
    public class ProductoLogica : IProducto
    {
        private readonly ContextoDb db;
        private readonly CProductoValidator validatorC;
        private readonly UProductoValidator validatorU;

        private static readonly Regex SoloDigitosRegex = new("\\D+", RegexOptions.Compiled);

        public ProductoLogica(ContextoDb _db)
        {
            db = _db;
            validatorC = new CProductoValidator();
            validatorU = new UProductoValidator();
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CProductoDto;
            var v = await validatorC.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            string normalizarDes = dto.Descripcion.Trim().ToUpperInvariant();

            var existe = await db.TaProductoModel.AsNoTracking().FirstOrDefaultAsync(x => x.Descripcion == normalizarDes && x.Vigente == true);

            if (existe != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Ya existe un producto con la misma descripción.");

            var model = new TaProductoModel
            {
                ProductoId = Guid.NewGuid().ToString(),
                CategoriaId = dto.CategoriaId.Trim(),
                Descripcion = normalizarDes,
                Precio = dto!.Precio,
                Vigente = true
            };

            db.Add(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ActualizarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as UProductoDto;
            var v = await validatorU.ValidateAsync(dto);
            if (!v.IsValid)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, v.ToString());

            var model = await db.TaProductoModel.FindAsync(dto!.ProductoId);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El producto no existe.");

            model.CategoriaId = string.IsNullOrWhiteSpace(dto.CategoriaId) ? model.CategoriaId : dto.CategoriaId;
            model.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? model.Descripcion : dto.Descripcion.Trim().ToUpperInvariant();
            if (dto.Precio.HasValue) model.Precio = dto.Precio.Value;

            db.Update(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as EliminarDto;

            if (string.IsNullOrWhiteSpace(dto.Id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.TaProductoModel.FindAsync(dto.Id);

            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El producto no existe.");

            model.Vigente = false;
            db.Update(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaProductoModel
                .Where(x => x.Vigente == true)
                .Select(p => new RProductoDto
                {
                    ProductoId = p.ProductoId,
                    CategoriaId = p.CategoriaId,
                    CategoriaIdStr = p.TaDominioModel.Descripcion,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio
                })
                .OrderBy(o => o.CategoriaIdStr)
                .ThenBy(O => O.Descripcion)
                .ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RProductoDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }

        public async Task<RespuestaDto<TReturn>> ExportarExcelAsync<TReturn>()
        {
            var productos = await db.TaProductoModel
                .Where(p => p.Vigente)
                .OrderBy(p => p.Descripcion)
                .Select(p => new { p.ProductoId, p.Descripcion, p.Precio })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Productos");

            ws.Cell(1, 1).Value = "ProductoId";
            ws.Cell(1, 2).Value = "Descripcion";
            ws.Cell(1, 3).Value = "Precio";

            ws.Column(3).Style.NumberFormat.Format = "0";

            for (var i = 0; i < productos.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = productos[i].ProductoId;
                ws.Cell(row, 2).Value = productos[i].Descripcion;
                ws.Cell(row, 3).Value = productos[i].Precio;
            }

            ws.Column(1).Hide();
            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)(object)stream.ToArray());
        }

        public async Task<RespuestaDto<TReturn>> ImportarExcelAsync<TParam, TReturn>(TParam _param)
        {
            var excelStream = _param as Stream;

            if (excelStream == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Archivo inválido.");

            using var wb = new XLWorkbook(excelStream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "No se encontró ninguna hoja en el archivo.");

            var used = ws.RangeUsed();
            if (used == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El archivo no contiene datos.");

            var lastRowNumber = used.LastRow().RowNumber();
            var lastColNumber = used.LastColumn().ColumnNumber();

            int? colId = null;
            int? colDesc = null;
            int? colPrecio = null;

            for (var c = 1; c <= lastColNumber; c++)
            {
                var header = ws.Cell(1, c).GetString().Trim();
                if (header.Equals("ProductoId", StringComparison.OrdinalIgnoreCase)) colId = c;
                else if (header.Equals("Descripcion", StringComparison.OrdinalIgnoreCase)) colDesc = c;
                else if (header.Equals("Precio", StringComparison.OrdinalIgnoreCase)) colPrecio = c;
            }

            if (colId == null || colDesc == null || colPrecio == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El archivo debe contener las columnas ProductoId, Descripcion y Precio en la primera fila.");

            var filas = new List<(string Id, string? Desc, decimal? Precio)>();
            var filasInvalidas = 0;

            for (var r = 2; r <= lastRowNumber; r++)
            {
                var id = ws.Cell(r, colId.Value).GetString().Trim();
                var descRaw = ws.Cell(r, colDesc.Value).GetString();

                if (string.IsNullOrWhiteSpace(id))
                {
                    if (!string.IsNullOrWhiteSpace(descRaw) || !ws.Cell(r, colPrecio.Value).IsEmpty())
                        filasInvalidas++;
                    continue;
                }

                var desc = NormalizeDescripcion(descRaw);
                if (string.IsNullOrWhiteSpace(desc))
                {
                    filasInvalidas++;
                    continue;
                }
                if (!TryGetPrecio(ws.Cell(r, colPrecio.Value), out var precio))
                {
                    filasInvalidas++;
                    continue;
                }

                filas.Add((id, desc, precio));
            }

            var ids = filas.Select(f => f.Id).Distinct().ToList();
            var productos = await db.TaProductoModel
                .Where(p => ids.Contains(p.ProductoId))
                .ToDictionaryAsync(p => p.ProductoId);

            var actualizados = 0;
            var sinCambios = 0;
            var noEncontrados = 0;

            foreach (var fila in filas)
            {
                if (!productos.TryGetValue(fila.Id, out var model))
                {
                    noEncontrados++;
                    continue;
                }

                if (!model.Vigente)
                {
                    noEncontrados++;
                    continue;
                }

                var cambio = false;

                var nuevaDescripcion = fila.Desc ?? string.Empty;
                var descActual = NormalizeDescripcion(model.Descripcion);
                if (!string.Equals(descActual, nuevaDescripcion, StringComparison.Ordinal))
                {
                    model.Descripcion = nuevaDescripcion;
                    cambio = true;
                }

                if (fila.Precio.HasValue && model.Precio != fila.Precio.Value)
                {
                    model.Precio = fila.Precio.Value;
                    cambio = true;
                }

                if (cambio) actualizados++;
                else sinCambios++;
            }

            if (actualizados > 0)
                await db.SaveChangesAsync();

            var result = new ProductoExcelImportResultDto
            {
                TotalFilas = filas.Count,
                Actualizados = actualizados,
                SinCambios = sinCambios,
                NoEncontrados = noEncontrados,
                FilasInvalidas = filasInvalidas
            };

            return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)(object)result);
        }

        private static string NormalizeDescripcion(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private static bool TryGetPrecio(IXLCell cell, out decimal? precio)
        {
            precio = null;

            if (cell.IsEmpty())
                return false;

            if (cell.DataType == XLDataType.Number)
            {
                var valor = cell.GetValue<decimal>();
                if (valor != decimal.Truncate(valor))
                    return false;

                precio = decimal.Truncate(valor);
                return true;
            }

            var raw = cell.GetString();
            var clean = SoloDigitosRegex.Replace(raw ?? string.Empty, string.Empty);
            if (string.IsNullOrWhiteSpace(clean))
                return false;

            if (!long.TryParse(clean, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
                return false;

            precio = parsed;
            return true;
        }
    }
}
