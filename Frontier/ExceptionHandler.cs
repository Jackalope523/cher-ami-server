using CrazyLizard.Exceptions;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace CrazyLizard
{
    public class ExceptionHandler(ILogger logger, IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.Error("\nHollow Exception\n{Message}\n{Trace}", exception.Message, exception.StackTrace);

            httpContext.Response.StatusCode = exception switch
            {
                AuthenticationException => StatusCodes.Status401Unauthorized,
                InvalidJwtException => StatusCodes.Status401Unauthorized,
                ValidationException => StatusCodes.Status400BadRequest,
                NoPermissionException => StatusCodes.Status403Forbidden,
                DeleteException => StatusCodes.Status403Forbidden,
                NoAccessException => StatusCodes.Status404NotFound,
                NotFoundException => StatusCodes.Status404NotFound,
                LockedOutException => StatusCodes.Status423Locked,
                NotImplementedException => StatusCodes.Status500InternalServerError,
                HttpRequestException => StatusCodes.Status502BadGateway,
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
