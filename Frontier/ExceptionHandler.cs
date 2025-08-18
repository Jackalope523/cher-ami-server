using Frontier.Exceptions;
using LazyLizardBackend.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace Frontier
{
    public class ExceptionHandler(ILogger logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.Error("\nHollow Exception\n{Message}\n{Trace}", exception.Message, exception.StackTrace);

            httpContext.Response.StatusCode = exception switch
            {
                UserErrorException => StatusCodes.Status400BadRequest,
                NoAccessException => StatusCodes.Status403Forbidden,
                DeleteException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Detail = $"{exception.GetType().Name}: {exception.Message}",
            };

            var context = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails,
            };

            return await problemDetailsService.TryWriteAsync(context);
        }
    }
}
