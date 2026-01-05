using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;
using Negocio.Utilidad;
using Negocio.Validador;

namespace Negocio.Gestion
{
    public class ProductoLogica : IProducto
    {
        private readonly ContextoDb db;
        private readonly CProductoValidator validatorC;
        private readonly UProductoValidator validatorU;

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

            var model = new TaProductoModel
            {
                ProductoId = Guid.NewGuid().ToString(),
                CategoriaId = dto!.CategoriaId!.Trim(),
                Descripcion = dto!.Descripcion!.Trim(),
                Precio = dto!.Precio!.Value,
                Vigente = true
            };

            db.Add(model);
            bool ok = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
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

            model.CategoriaId = string.IsNullOrWhiteSpace(dto.CategoriaId) ? model.CategoriaId : dto.CategoriaId.Trim();
            model.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? model.Descripcion : dto.Descripcion.Trim();
            if (dto.Precio.HasValue) model.Precio = dto.Precio.Value;

            db.Update(model);
            bool ok = await new GestionAuditoriaLogica(db).SaveChangesAsync(dto.ParametrosAuditoriaDto);

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> EliminarAsync<TParam, TReturn>(TParam _param)
        {
            var id = _param as string;
            if (string.IsNullOrWhiteSpace(id))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "Identificador inválido.");

            var model = await db.TaProductoModel.FindAsync(id);
            if (model == null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El producto no existe.");

            model.Vigente = false;
            db.Update(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok) return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarListaAsync<TReturn>()
        {
            var resultados = await db.TaProductoModel.Select(p => new RProductoDto
            {
                ProductoId = p.ProductoId,
                CategoriaId = p.CategoriaId,
                CategoriaIdStr = p.TaDominioModel.Descripcion,
                Descripcion = p.Descripcion,
                Precio = p.Precio
            }).OrderBy(o => o.Descripcion).ToListAsync();

            if (resultados.Count != 0)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(resultados, typeof(List<RProductoDto>)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa");
        }
    }
}
