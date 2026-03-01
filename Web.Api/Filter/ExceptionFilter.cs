using Comun.Dto;
using Comun.Enumeracion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Api.Filtro
{
    public class ExceptionFilter : IActionFilter
    {
    

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

            var response = new RespuestaDto<string>
            {
                Codigo = EstadoOperacion.Exception,
                Mensaje = $"Tuvimos un inconveniente con su solicitud. Por favor tome contacto con soporte: {exception.Message}.",
                Respuesta = null
            };

            context.Result = new ObjectResult(response);
            context.ExceptionHandled = true;
        }

    }

}