using Comun.Dto;
using Comun.Dto.DtoParameter;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;

namespace Negocio.Gestion
{
    public class MenuCartaLogica : IMenuCarta
    {
        private readonly ContextoDb db;

        public MenuCartaLogica(ContextoDb _db)
        {
            db = _db;
        }

        public async Task<RespuestaDto<TReturn>> GuardarAsync<TParam, TReturn>(TParam _param)
        {
            var dto = _param as CMenuCartaDto;

            if (dto == null || string.IsNullOrWhiteSpace(dto.Contenido))
                return new RespuestaDto<TReturn>(EstadoOperacion.Validacion, "El contenido del archivo es obligatorio.");

            // Solo se admite un menú (PDF) vigente: se reemplaza el anterior.
            var vigentes = await db.TaMenuCartaModel
                .Where(x => x.Vigente)
                .ToListAsync();

            foreach (var v in vigentes)
                v.Vigente = false;

            var model = new TaMenuCartaModel
            {
                MenuCartaId = Guid.NewGuid().ToString(),
                NombreArchivo = string.IsNullOrWhiteSpace(dto.NombreArchivo) ? "menu.pdf" : dto.NombreArchivo.Trim(),
                Contenido = dto.Contenido,
                FechaRegistro = DateTime.Now,
                Vigente = true
            };

            db.Add(model);
            bool ok = await db.SaveChangesAsync() > 0;

            if (ok)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación realizada correctamente.");
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "Operación no exitosa.");
        }

        public async Task<RespuestaDto<TReturn>> ConsultarActualAsync<TReturn>()
        {
            var actual = await db.TaMenuCartaModel
                .AsNoTracking()
                .Where(x => x.Vigente)
                .OrderByDescending(x => x.FechaRegistro)
                .Select(x => new RMenuCartaDto
                {
                    MenuCartaId = x.MenuCartaId,
                    NombreArchivo = x.NombreArchivo,
                    Contenido = x.Contenido,
                    FechaRegistro = x.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (actual != null)
                return new RespuestaDto<TReturn>(EstadoOperacion.Bueno, "Operación exitosa", (TReturn)Convert.ChangeType(actual, typeof(RMenuCartaDto)));
            return new RespuestaDto<TReturn>(EstadoOperacion.Malo, "No hay un menú disponible.");
        }
    }
}
