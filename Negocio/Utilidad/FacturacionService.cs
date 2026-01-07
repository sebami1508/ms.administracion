using Datos.Orm.Contexto;
using Datos.Orm.Entidades;
using Microsoft.EntityFrameworkCore;
using Negocio.Contrato;

public class FacturacionService : IFacturacionService
{
    private readonly ContextoDb db;

    public FacturacionService(ContextoDb _db)
    {
        db = _db;
    }

    public async Task<string> GenerarNumeroFacturaAsync(string prefijo = "BRA-")
    {
        var anioActual = DateTime.UtcNow.Year;

        const int maxIntentos = 3;

        for (int intento = 1; intento <= maxIntentos; intento++)
        {
            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                var consecutivo = await db.TaConsecutivoFacturaModel
                    .SingleOrDefaultAsync(c => c.Prefijo == prefijo && c.Anio == anioActual);

                if (consecutivo is null)
                {
                    consecutivo = new TaConsecutivoFacturaModel
                    {
                        Prefijo = prefijo,
                        Anio = anioActual,
                        UltimoNumero = 1,
                        FechaActualizacion = DateTime.UtcNow
                    };

                    db.TaConsecutivoFacturaModel.Add(consecutivo);
                }
                else
                {
                    consecutivo.UltimoNumero++;
                    consecutivo.FechaActualizacion = DateTime.UtcNow;
                }

                await db.SaveChangesAsync();
                await tx.CommitAsync();

                return $"{prefijo}{consecutivo.UltimoNumero:D6}";
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();

                if (intento == maxIntentos)
                    throw new InvalidOperationException("No fue posible generar el número de factura después de varios intentos.", ex);
            }
        }

        // no debería llegar acá, pero por si acaso
        throw new InvalidOperationException("No fue posible generar el número de factura.");
    }
}
