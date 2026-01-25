

using ESSPortal.Domain.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using StackExchange.Redis;

using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EssPortal.Web.Mvc.Middleware;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger = logger;

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
                case ServiceUnavailableException:
                    problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                    problemDetails.Title = "Service Unavailable";
                    problemDetails.Detail = exception.Message;
                    break;

                case ResourceNotFoundException:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "Resource Not Found";
                    problemDetails.Detail = exception.Message;
                    break;

                case ValidationException validationException:
                    problemDetails.Status = (int)HttpStatusCode.UnprocessableEntity;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Detail = validationException.ValidationResult.ErrorMessage;

                    var errors = new Dictionary<string, string[]>();
                    foreach (var memberName in validationException.ValidationResult.MemberNames)
                    {
                        errors.Add(memberName, [validationException.ValidationResult.ErrorMessage!]);
                    }

                    problemDetails.Extensions["errors"] = errors;
                    break;

                case FluentValidation.ValidationException fluentEx:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Validation Error";
                    problemDetails.Detail = "One or more validation errors occurred.";

                    var validationErrors = fluentEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());

                    problemDetails.Extensions["errors"] = validationErrors;
                    break;

                case CreatingDuplicateException:
                    problemDetails.Status = StatusCodes.Status409Conflict;
                    problemDetails.Title = "Conflict";
                    problemDetails.Detail = "A resource with the same identifier already exists.";
                    break;

                case HttpRequestException httpRequestException:
                    problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                    problemDetails.Title = "Service Unavailable";
                    problemDetails.Detail = httpRequestException.Message;
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
            throw; 
        }
    }

}
