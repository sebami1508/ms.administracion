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
        if (string.IsNullOrWhiteSpace(prefijo))
            throw new ArgumentException("El prefijo es requerido.", nameof(prefijo));

        // timestamp without time zone → DateTimeKind.Unspecified
        static DateTime NowUnspecified() =>
            DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        var anioActual = DateTime.Now.Year;

        const int maxIntentos = 3;

        for (int intento = 1; intento <= maxIntentos; intento++)
        {
            await using var tx = await db.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                var consecutivo = await db.TaConsecutivoFacturaModel
                    .SingleOrDefaultAsync(c =>
                        c.Prefijo == prefijo &&
                        c.Anio == anioActual);

                if (consecutivo == null)
                {
                    consecutivo = new TaConsecutivoFacturaModel
                    {
                        Prefijo = prefijo,
                        Anio = anioActual,
                        UltimoNumero = 1,
                        FechaActualizacion = NowUnspecified()
                    };

                    db.TaConsecutivoFacturaModel.Add(consecutivo);
                }
                else
                {
                    consecutivo.UltimoNumero++;
                    consecutivo.FechaActualizacion = NowUnspecified();
                }

                await db.SaveChangesAsync();
                await tx.CommitAsync();

                return $"{prefijo}{consecutivo.UltimoNumero:D6}";
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();

                // Reintenta solo si es una colisión de concurrencia
                if (intento == maxIntentos)
                    throw new InvalidOperationException(
                        "No fue posible generar el número de factura después de varios intentos por concurrencia."
                    );
            }
        }

        throw new InvalidOperationException("No fue posible generar el número de factura.");
    }

}
