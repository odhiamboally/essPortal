using EssPortal.Web.Mvc.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;

using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EssPortal.Web.Mvc.Middleware;

public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;

    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogError(exception, "An error occurred while handling an exception.");

            var problemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Instance = httpContext.Request.Path,
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "An error occurred while processing your request."
            };


            switch (exception)
            {
                case HttpRequestException httpRequestException:
                    problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                    problemDetails.Title = "Service Unavailable";
                    problemDetails.Detail = httpRequestException.Message;
                    break;

                case NoContentException:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "No Records Found";
                    problemDetails.Detail = "The requested resource could not be found.";
                    break;

                case CreatingDuplicateException:
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    problemDetails.Title = "Duplicate Record";
                    problemDetails.Detail = "The record already exists in the system.";
                    break;

                case ValidationException validationException:
                    problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Detail = validationException.ValidationResult.ErrorMessage;
                    problemDetails.Extensions["ValidationErrors"] = validationException.ValidationResult.MemberNames
                        .ToDictionary(m => m, _ => new[] { validationException.ValidationResult.ErrorMessage });
                    break;

                case FluentValidation.ValidationException fluentValidationException:
                    problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Detail = "One or more validation errors occurred.";
                    problemDetails.Extensions["errors"] = fluentValidationException.Errors
                        .ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                    break;

                case RedisConnectionException redisConnectionException:
                    problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                    problemDetails.Title = "Cache Service Unavailable";
                    problemDetails.Detail = "An error occurred while communicating with the cache service. Please try again later.";
                    _logger.LogError(redisConnectionException, "Redis connection error occurred.");
                    break;

                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "An unexpected error occurred";
                    problemDetails.Detail = "An error occurred while processing your request.";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling another exception.");
            throw; // Re-throw the exception to allow other handlers or the default error handling middleware to process it
        }
    }

}
