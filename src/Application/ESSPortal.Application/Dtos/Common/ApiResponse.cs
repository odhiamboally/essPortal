using System.Text.Json.Serialization;

namespace ESSPortal.Application.Dtos.Common;

public record ApiResponse<T>
{
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; init; }
    public T? Data { get; init; }
    public List<string> Errors { get; set; } = new();
    public string? ErrorCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    [JsonIgnore]
    public Exception? Exception { get; init; }

    public ApiResponse()
    {
    }

    [JsonConstructor]
    private ApiResponse(bool successful, string? message, T? data, Exception? exception)
    {
        Successful = successful;
        Message = message ?? "Operation Successful";
        Data = data;
        Exception = exception;
    }

    

    public static ApiResponse<T> Success(string message, T data)
    {
        return new ApiResponse<T>
        {
            Successful = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Successful = true,
            Data = data
        };
    }

    public static ApiResponse<T> Success(string message, T value, Exception? exception = null)
    {
        return new ApiResponse<T>(true, message, value, exception);
    }

    public static ApiResponse<T> Failure(string errorMessage, T? data = default, Exception? error = null)
    {
        return new ApiResponse<T>(false, errorMessage, data, error);
    }

    public static ApiResponse<T> ValidationFailure(Dictionary<string, List<string>> validationErrors)
    {
        var allErrors = validationErrors.SelectMany(kvp =>
            kvp.Value.Select(error => $"{kvp.Key}: {error}")).ToList();

        return new ApiResponse<T>
        {
            Successful = false,
            Message = "Validation failed",
            Errors = allErrors,
            ErrorCode = "VALIDATION_ERROR"
        };
    }
}
