using Comun.Dto;
using Comun.Enumeracion;
using Datos.Orm.Contexto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Negocio.Utilidad;
using Npgsql;
using System.Security.Cryptography;

namespace Web.Api.Filtro
{
    public class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly DbContextError db;
        public int Order => int.MaxValue - 1;

        public ExceptionFilter(DbContextError _db)
        {
            db = _db;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                HandleException(context);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context) { }

        private void HandleException(ActionExecutedContext context)
        {
            var exception = context.Exception;
            string codigoUnico = HttpStatusMapper.GenerarIdentificador();
            int statusCode = HttpStatusMapper.GetStatusCode(exception);

            var response = new RespuestaDto<string>
            {
                Codigo = EstadoOperacion.Exception,
                Mensaje = $"Tuvimos un inconveniente con su solicitud. Identificador de seguimiento: {codigoUnico}. Por favor comparte este código con soporte.",
                Respuesta = $"Message: {exception.Message} => InnerException: {exception.InnerException} => StackTrace: {exception.StackTrace}"
            };

            new GestionErrorLogica(db).SaveError(response, codigoUnico, statusCode);

            context.Result = new ObjectResult(response) { StatusCode = statusCode };
            context.ExceptionHandled = true;
        }

    }

    public class ForbiddenAccessException(string message) : Exception(message);

    public class ConflictException(string message) : Exception(message);

    public class DatabaseException : Exception
    {
        public int ErrorCode { get; }

        public DatabaseException(string message, Exception innerException, int errorCode = 500)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class HttpStatusMapper
    {
        private static readonly Dictionary<Type, int> ExceptionStatusCodes = new()
        {
            { typeof(ArgumentException), StatusCodes.Status400BadRequest },
            { typeof(FormatException), StatusCodes.Status400BadRequest },
            { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
            { typeof(ForbiddenAccessException), StatusCodes.Status403Forbidden },
            { typeof(KeyNotFoundException), StatusCodes.Status404NotFound },
            { typeof(ConflictException), StatusCodes.Status409Conflict },
            { typeof(TimeoutException), StatusCodes.Status408RequestTimeout },
            { typeof(DatabaseException), StatusCodes.Status503ServiceUnavailable }
        };

        public static int GetStatusCode(Exception exception)
        {
            // Desempaquetar AggregateException (caso Task/async) si solo trae una inner
            while (exception is AggregateException agg && agg.InnerExceptions.Count == 1)
                exception = agg.InnerExceptions[0];

            // Si existe una InnerException más específica, la usamos para determinar el código
            var inner = exception.InnerException;

            // EF Core: DbUpdateException suele envolver PostgresException
            if (exception is DbUpdateException dbUpdate && dbUpdate.InnerException is PostgresException pgFromUpdate)
                return MapPostgresException(pgFromUpdate);

            // InnerException directa
            if (inner is PostgresException innerPg)
                return MapPostgresException(innerPg);

            // Excepción PG directa
            if (exception is PostgresException directPg)
                return MapPostgresException(directPg);

            // Problemas de conexión/timeout de Npgsql (no siempre trae SqlState)
            if (exception is NpgsqlException)
                return StatusCodes.Status503ServiceUnavailable;

            // Concurrency en EF → 409
            if (exception is DbUpdateConcurrencyException)
                return StatusCodes.Status409Conflict;

            // Validaciones (FluentValidation) → 400
            if (exception.GetType().FullName == "FluentValidation.ValidationException")
                return StatusCodes.Status400BadRequest;

            // Tipos comunes adicionales
            if (exception is NotSupportedException)
                return StatusCodes.Status501NotImplemented;

            if (exception is OperationCanceledException)
                return StatusCodes.Status408RequestTimeout;

            if (exception is NullReferenceException)
                return StatusCodes.Status500InternalServerError;

            return ExceptionStatusCodes.TryGetValue(exception.GetType(), out int statusCode)
                ? statusCode
                : StatusCodes.Status500InternalServerError;
        }

        private static int MapPostgresException(PostgresException ex)
        {
            return ex.SqlState switch
            {
                "23505" => StatusCodes.Status409Conflict,              // UNIQUE / PK duplicada
                "23503" => StatusCodes.Status409Conflict,              // FK violation
                "23514" => StatusCodes.Status409Conflict,              // CHECK violation (integridad)
                "23502" => StatusCodes.Status422UnprocessableEntity,   // NOT NULL violation
                "42P01" => StatusCodes.Status404NotFound,              // tabla/vista no existe
                "42501" => StatusCodes.Status403Forbidden,             // permisos insuficientes
                "28P01" => StatusCodes.Status401Unauthorized,          // auth (password inválida)
                "40P01" => StatusCodes.Status409Conflict,              // deadlock_detected
                "57014" => StatusCodes.Status408RequestTimeout,        // query_canceled/cancelled
                _ => StatusCodes.Status500InternalServerError
            };
        }

        public static string GenerarIdentificador()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using var rng = RandomNumberGenerator.Create();

            byte[] randomBytes = new byte[8];
            rng.GetBytes(randomBytes);

            return new string(randomBytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }

}