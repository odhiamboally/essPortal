using System.Text.Json.Serialization;

namespace ESSPortal.Shared.Dtos.Common;

public record AppResponse<T>
{
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }
    public string? ErrorCode { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? TraceId { get; set; }

    [JsonIgnore]
    public Exception? Exception { get; init; }
    public List<string> Errors { get; set; } = [];
    public Dictionary<string, string>? Headers { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }

    public AppResponse()
    {
    }

    [JsonConstructor]
    private AppResponse(bool successful, string? message, T? data, Exception? exception)
    {
        Successful = successful;
        Message = message ?? "Operation Successful";
        Data = data;
        Exception = exception;
    }

    

    public static AppResponse<T> Success(string message, T data)
    {
        return new AppResponse<T>
        {
            Successful = true,
            Message = message,
            Data = data
        };
    }

    public static AppResponse<T> Success(T data)
    {
        return new AppResponse<T>
        {
            Successful = true,
            Data = data
        };
    }

    public static AppResponse<T> Success(string message, T value, Exception? exception = null)
    {
        return new AppResponse<T>(true, message, value, exception);
    }

    public static AppResponse<T> Failure(string errorMessage, T? data = default, Exception? error = null)
    {
        return new AppResponse<T>(false, errorMessage, data, error);
    }

    public static AppResponse<T> ValidationFailure(Dictionary<string, List<string>> validationErrors)
    {
        var allErrors = validationErrors.SelectMany(kvp =>
            kvp.Value.Select(error => $"{kvp.Key}: {error}")).ToList();

        return new AppResponse<T>
        {
            Successful = false,
            Message = "Validation failed",
            Errors = allErrors,
            ErrorCode = "VALIDATION_ERROR"
        };
    }
}
