using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Negocio;

namespace Controllers;

public class FiltroErroresNegocio : IExceptionFilter
{
    public void OnException(ExceptionContext contexto)
    {
        switch (contexto.Exception)
        {
            case ArgumentException error:
                contexto.Result = new BadRequestObjectResult(new { mensaje = error.Message });
                contexto.ExceptionHandled = true;
                break;
            case NegocioConflictoException error:
                contexto.Result = new ConflictObjectResult(new { mensaje = error.Message });
                contexto.ExceptionHandled = true;
                break;
        }
    }
}
