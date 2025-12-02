using System.Text.Json.Serialization;

namespace EssPortal.Web.Mvc.Dtos.Common;

public class AppResponse<T>
{
    public bool Successful { get; init; }
    public string? Message { get; init; }
    public string? SessionId { get; set; }
    public T? Data { get; init; }
    public Exception? Exception { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
    

    [JsonConstructor]
    private AppResponse(bool successful, string? message, T? data, Exception? exception, Dictionary<string, object>? additionalData)
    {
        Successful = successful;
        Message = message ?? "Operation Successful";
        Data = data;
        Exception = exception;
        AdditionalData = additionalData ?? new Dictionary<string, object>();
    }

    public static AppResponse<T> Success(string message, T value, Exception? exception = null, Dictionary<string, object>? additionalData = null)
    {
        return new AppResponse<T>(true, message, value, exception, additionalData);
    }

    public static AppResponse<T> Failure(string errorMessage, T? data = default, Exception? error = null, Dictionary<string, object>? additionalData = null)
    {
        return new AppResponse<T>(false, errorMessage, data, error, additionalData);
    }
}
