using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using StackExchange.Redis;
using EssPortal.Web.Mvc.Exceptions;

namespace EssPortal.Web.Mvc.Utilities.ExceptionHandling;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Exception occurred in the MVC application.");

        var problemDetails = new ProblemDetails
        {
            Type = context.Exception.GetType().Name,
            Instance = context.HttpContext.Request.Path
        };

        switch (context.Exception)
        {
            case HttpRequestException:
                problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                problemDetails.Title = "Service Unavailable";
                problemDetails.Detail = "Unable to connect to the server. Please try again later.";
                problemDetails.Extensions["errors"] = context.Exception?.Message;
                break;

            case InvalidOperationException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Operation";
                problemDetails.Detail = "There was a problem with the request being sent.";
                break;

            case CreatingDuplicateException:
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Title = "Duplicate Record";
                problemDetails.Detail = "The record already exists in the system.";
                break;

            case ValidationException validationException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = validationException.ValidationResult.ErrorMessage;
                break;

            case FluentValidation.ValidationException fluentValidationException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
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
                problemDetails.Detail = "Please contact the system admin.";
                problemDetails.Extensions["errors"] = context.Exception?.Message;
                break;
        }

        context.HttpContext.Response.StatusCode = problemDetails.Status.Value;
        context.Result = new RedirectToActionResult("Error", "Home", new
        {
            errorTitle = problemDetails.Title,
            errorMessage = problemDetails.Detail,
            referenceCode = problemDetails.Status.ToString(),
            details = problemDetails.Extensions["errors"]
        });

        context.ExceptionHandled = true;
    }
}

