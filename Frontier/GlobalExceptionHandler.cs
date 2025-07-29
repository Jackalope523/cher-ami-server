using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace Frontier
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger _logger;

        public GlobalExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";

            ErrorShard errorResponse;

            switch(exception)
            {
                case UserErrorException ex:
                    _logger.Debug("\nUser Exception\n{message}\n{trace}", ex.Message, ex.StackTrace);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = new ErrorShard(HttpStatusCode.BadRequest, "Something is wrong with your request.");
                    break;

                case HollowException ex:
                    _logger.Error("\nHollow Exception\n{message}\n{trace}", ex.Message, ex.StackTrace);
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorShard(HttpStatusCode.InternalServerError, "Something failed on the server.");
                    break;

                default:
                    _logger.Error(exception, "Unhandled exception");
                    httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new ErrorShard(HttpStatusCode.InternalServerError, "An unexpected error occurred.");
                    break;
            }

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
