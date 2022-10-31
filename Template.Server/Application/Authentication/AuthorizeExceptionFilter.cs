namespace Template.Server.Application.Authentication;

using Microsoft.AspNetCore.Mvc.Filters;

public sealed class AuthorizeExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is AuthorizeException ex)
        {
            context.Result = new StatusCodeResult(ex.StatusCode);
            context.ExceptionHandled = true;
        }
    }
}
