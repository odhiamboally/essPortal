using EssPortal.Web.Mvc.Configurations;
using EssPortal.Web.Mvc.Dtos.Common;
using EssPortal.Web.Mvc.Enums.NavEnums;

using ESSPortal.Web.Mvc.Contracts.Interfaces.Common;
using ESSPortal.Web.Mvc.Contracts.Interfaces.Services;
using ESSPortal.Web.Mvc.Utilities.Common;

using Jose;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ESSPortal.Web.Mvc.Contracts.Implementations.Common;

internal sealed class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<ApiService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPayloadEncryptionService _payloadEncryptionService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, IOptions<ApiSettings> appSettings, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor, IPayloadEncryptionService payloadEncryptionService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiSettings = appSettings.Value ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _payloadEncryptionService = payloadEncryptionService ?? throw new ArgumentNullException(nameof(payloadEncryptionService));

        // CRITICAL: Validate BaseAddress is set
        if (_httpClient.BaseAddress == null)
        {
            var baseUrl = _apiSettings.BaseUrl ?? "https://localhost:7291/";
            _logger.LogWarning("HttpClient BaseAddress is null. Setting to: {BaseUrl}", baseUrl);
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        _logger.LogInformation("ApiService initialized with BaseAddress: {BaseAddress}", _httpClient.BaseAddress);
        _jsonOptions = CreateResilientJsonOptions();
    }

    public async Task<AppResponse<TResponse?>> DeleteAsync<TResponse>(string endpoint)
    {
        string responseMessage = string.Empty;
        try
        {
            
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }

            SetAuthorizationHeader();

            var apiResponse = await _httpClient.DeleteAsync(endpoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var content = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(content);

                // Provide context-appropriate error messages
                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, content);

                return AppResponse<TResponse?>.Failure(contextualError);

            }

            var response = await apiResponse.Content.ReadFromJsonAsync<AppResponse<TResponse>>();
            if (response == null)
            {
                responseMessage = "Response content is null";
                return AppResponse<TResponse?>.Failure("Response is null");
            }

            responseMessage = response.Message ?? "Resource deleted successfully";
            return AppResponse<TResponse?>.Success(response.Message!, response.Data);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request failed for endpoint: {Endpoint}", endpoint);
            return AppResponse<TResponse?>.Failure(responseMessage);
        }
    }

    public async Task<AppResponse<TResponse?>> GetAsync<TRequest, TResponse>(string endpoint, TRequest? request = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }

            SetAuthorizationHeader();

            var apiResponse = await _httpClient.GetAsync(endpoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var content = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(content);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, content);

                return AppResponse<TResponse?>.Failure(contextualError);
            }

            
            var responseContent = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent =_payloadEncryptionService.Decrypt(responseContent);
            var response = JsonSerializer.Deserialize<AppResponse<TResponse>>(decryptedContent, _jsonOptions);

            //var response = await apiResponse.Content.ReadFromJsonAsync<AppResponse<TResponse>>();

            if (response == null)
            {
                return AppResponse<TResponse?>.Failure("Response content is null");
            }

            return AppResponse<TResponse?>.Success(response.Message!, response.Data);
            
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<AppResponse<TResponse?>> GetAsync<TResponse>(string endpoint)
    {
        string responseMessage = string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }

            SetAuthorizationHeader();

            var apiResponse = await _httpClient.GetAsync(endpoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorContent = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(errorContent);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, errorContent);

                return AppResponse<TResponse?>.Failure(contextualError);
            }

            var content = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent = _payloadEncryptionService.Decrypt(content);
            var response = JsonSerializer.Deserialize<AppResponse<TResponse>>(decryptedContent, _jsonOptions);

            if (response == null)
            {
                responseMessage = "Response content is null";
                return AppResponse<TResponse?>.Failure("Response content is null");
            }

            responseMessage = response.Message ?? "Records fetched successfully";
            return AppResponse<TResponse?>.Success(response.Message!, response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed for endpoint: {Endpoint}", endpoint);
            return AppResponse<TResponse?>.Failure(responseMessage);
        }
    }

    public async Task<AppResponse<TResponse?>> PostAsync<TRequest, TResponse>(string endpoint, TRequest? request)
    {
        string responseMessage = string.Empty;
        string sessionId = string.Empty;
        
        try
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }

            // Ensure BaseAddress is set
            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = _apiSettings.BaseUrl ?? "https://localhost:7291/";
                _httpClient.BaseAddress = new Uri(baseUrl);
                _logger.LogWarning("BaseAddress was null, set to: {BaseUrl}", baseUrl);
            }

            var cleanEndpoint = endpoint.TrimStart('/');

            SetAuthorizationHeader();

            // Encrypt the request payload
            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var encryptedContent = _payloadEncryptionService.Encrypt(jsonContent);
            var content = new StringContent(encryptedContent, Encoding.UTF8, "application/json");

            var apiResponse = await _httpClient.PostAsync(endpoint, content);

            // Capture X-Session-Id header
            if (apiResponse.Headers.TryGetValues("X-Session-Id", out var sessionIdValues))
            {
                sessionId = sessionIdValues.FirstOrDefault() ?? string.Empty;

            }

            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorContent = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(errorContent);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, errorContent);

                return AppResponse<TResponse?>.Failure(contextualError);
            }

            var responseContent = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent = _payloadEncryptionService.Decrypt(responseContent);
            var response = JsonSerializer.Deserialize<AppResponse<TResponse>>(decryptedContent, _jsonOptions);

            if (response == null)
            {
                responseMessage = "Response content is null";
                return AppResponse<TResponse?>.Failure("Response content is null");
            }

            response.SessionId = sessionId;

            return AppResponse<TResponse?>.Success(response.Message!, response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed for endpoint: {Endpoint}", endpoint);
            return AppResponse<TResponse?>.Failure("An error occurred while processing your request.");
        }
    }

    public async Task<AppResponse<TResponse?>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        string responseMessage = string.Empty;
        try
        {
            SetAuthorizationHeader();

            // Encrypt the request payload
            var jsonContent = JsonSerializer.Serialize(request, _jsonOptions);
            var encryptedContent = _payloadEncryptionService.Encrypt(jsonContent);
            var content = new StringContent(encryptedContent, Encoding.UTF8, "application/json");

            var apiResponse = await _httpClient.PutAsync(endpoint, content);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorContent = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(errorContent);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, errorContent);

                return AppResponse<TResponse?>.Failure(contextualError);
            }

            var responseContent = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent = _payloadEncryptionService.Decrypt(responseContent);
            var response = JsonSerializer.Deserialize<AppResponse<TResponse>>(decryptedContent, _jsonOptions);

            if (response == null)
            {
                responseMessage = "Response content is null";
                return AppResponse<TResponse?>.Failure("Response content is null");
            }

            if (!response.Successful || response.Data == null)
            {
                responseMessage = response.Message ?? "Update operation failed";
                return AppResponse<TResponse?>.Failure(responseMessage);
            }

            responseMessage = response.Message ?? "Resource updated successfully";
            return AppResponse<TResponse?>.Success(response.Message!, response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT request failed for endpoint: {Endpoint}", endpoint);
            return AppResponse<TResponse?>.Failure(responseMessage);
        }
    }
    
    public async Task<AppResponse<PagedResult<TResponse>>> GetPagedAsync<TResponse>(string endpoint)
    {
        string content = string.Empty;
        try
        {
            SetAuthorizationHeader();

            var apiResponse = await _httpClient.GetAsync(endpoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                content = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(content);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, content);

                return AppResponse<PagedResult<TResponse>>.Failure(contextualError);
            }

            content = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent = _payloadEncryptionService.Decrypt(content);
            var response = JsonSerializer.Deserialize<AppResponse<PagedResult<TResponse>>>(decryptedContent, _jsonOptions);

            return response ?? AppResponse<PagedResult<TResponse>>.Failure("Failed to deserialize response");
        }
        catch (Exception ex)
        {
            return AppResponse<PagedResult<TResponse>>.Failure(ex.Message);
        }

    }

    public async Task<AppResponse<PagedResult<TResponse>>> GetPagedAsync<TRequest, TResponse>(string endpoint, TRequest? request)
    {
        try
        {
            SetAuthorizationHeader();

            var apiResponse = await _httpClient.GetAsync(endpoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var errorContent = await apiResponse.Content.ReadAsStringAsync();

                errorContent = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(errorContent);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, errorContent);
                
                return AppResponse<PagedResult<TResponse>>.Failure(contextualError);
            }

            var content = await apiResponse.Content.ReadAsStringAsync();
            var decryptedContent = _payloadEncryptionService.Decrypt(content);
            var response = JsonSerializer.Deserialize<AppResponse<PagedResult<TResponse>>>(decryptedContent, _jsonOptions);

            //var response = JsonSerializer.Deserialize<AppResponse<PagedResult<TResponse>>>(content, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});
            
            return response ?? AppResponse<PagedResult<TResponse>>.Failure("Failed to deserialize response");

        }
        catch (Exception ex)
        {
            return AppResponse<PagedResult<TResponse>>.Failure(ex.Message);
        }
    }

    public async Task<AppResponse<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest kpi)
    {
        string responseMessage = string.Empty;
        try
        {
            SetAuthorizationHeader();

            var apiResponse = await _httpClient.PatchAsJsonAsync(endpoint, kpi);
            if (!apiResponse.IsSuccessStatusCode)
            {
                var content = await apiResponse.Content.ReadAsStringAsync();
                var errorMessage = ExtractErrorMessage(content);

                var contextualError = GetErrorMessage(apiResponse.StatusCode, errorMessage, apiResponse.ReasonPhrase);

                _logger.LogWarning("API request failed. Status: {StatusCode}, Response: {Content}",
                    apiResponse.StatusCode, content);
                return AppResponse<TResponse>.Failure(contextualError);

            }
            var response = await apiResponse.Content.ReadFromJsonAsync<AppResponse<TResponse>>();
            if (response == null)
            {
                responseMessage = "Response content is null";
                return AppResponse<TResponse>.Failure("Response content is null");

            }
            if (!response.Successful || response.Data == null)
            {
                responseMessage = response.Message ?? "Patch operation failed";
                return AppResponse<TResponse>.Failure(response.Message ?? "Patch operation failed");
            }


            responseMessage = response.Message ?? "Patch operation successful";
            return AppResponse<TResponse>.Success(response.Message ?? "Patch operation successful", response.Data);


        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "PATCH request failed for endpoint: {Endpoint}", endpoint);
            return AppResponse<TResponse>.Failure(responseMessage);
        }
    }



    private void SetAuthorizationHeader()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var token = httpContext.Request.Cookies["auth_token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            token = httpContext.Session.GetString("auth_token");
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            if (!IsTokenValid(token))
            {
                _logger.LogWarning("Token is invalid or expired, clearing authorization header");
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // ALWAYS include session ID if available
        _httpClient.DefaultRequestHeaders.Remove("X-Session-Id");
        var sessionId = httpContext.Request.Cookies["session_id"];
        if (!string.IsNullOrEmpty(sessionId))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Session-Id", sessionId);
        }

    }

    private bool IsTokenValid(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
            {
                _logger.LogWarning("Token cannot be read by JWT handler");
                return false;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiry = jwtToken.ValidTo;
            var now = DateTime.UtcNow;

            if (expiry <= now)
            {
                _logger.LogWarning("Token has expired. Expiry: {Expiry}, Current time: {Now}", expiry, now);
                return false;
            }

            _logger.LogDebug("Token is valid. Expires at: {Expiry}", expiry);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    private string? ExtractErrorMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        // Try parsing as plaintext JSON
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (TryExtractMessage(doc.RootElement, out var message) && !string.IsNullOrEmpty(message))
                return message;
        }
        catch (JsonException)
        {
            // Not valid JSON — may be encrypted or plain text error;
        }

        // Try decrypting and then parsing as JSON
        string? decryptedContent = null;
        try
        {
            decryptedContent = _payloadEncryptionService.Decrypt(content);
        }
        catch (Exception ex) when (IsDecryptionException(ex))
        {
            // Decryption failed — treat as raw content
            _logger?.LogDebug(ex, "Failed to decrypt error content. Treating as raw.");
        }

        if (!string.IsNullOrEmpty(decryptedContent))
        {
            try
            {
                using var doc = JsonDocument.Parse(decryptedContent);
                if (TryExtractMessage(doc.RootElement, out var message) && !string.IsNullOrEmpty(message))
                    return message;
            }
            catch (JsonException ex)
            {
                _logger?.LogDebug(ex, "Decrypted content is not valid JSON. Falling back to raw.");
            }
        }

        // Fallback to raw content
        // Only return raw content if it's short and safe (avoid leaking sensitive/long data)
        return content.Length <= 200 ? content : "An error occurred.";
    }

    private string? ExtractErrorMessage_(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            // Try to decrypt first if it looks encrypted
            var decryptedContent = _payloadEncryptionService.Decrypt(content);

            // Try to parse as ApiResponse error format
            using var document = JsonDocument.Parse(decryptedContent);

            // Check for "message" field (backend ApiResponse format)
            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }

            // Check for "title" field (ProblemDetails format)
            if (document.RootElement.TryGetProperty("title", out var titleElement))
            {
                return titleElement.GetString();
            }

            // Check for "error" field
            if (document.RootElement.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString();
            }

            // Check for "errors" array (validation errors)
            if (document.RootElement.TryGetProperty("errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                var errors = new List<string>();
                foreach (var error in errorsElement.EnumerateArray())
                {
                    if (error.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(error.GetString()!);
                    }
                }
                return errors.Count > 0 ? string.Join("; ", errors) : null;
            }
        }
        catch (JsonException)
        {
            // If we can't parse as JSON, return the raw content if it's short enough
            return content.Length <= 200 ? content : "An error occurred.";
        }

        return null;
    }

    private static bool TryExtractMessage(JsonElement root, out string? message)
    {
        message = null;

        if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
        {
            message = m.GetString();
            return !string.IsNullOrEmpty(message);
        }

        if (root.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
        {
            message = t.GetString();
            return !string.IsNullOrEmpty(message);
        }

        if (root.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String)
        {
            message = e.GetString();
            return !string.IsNullOrEmpty(message);
        }

        if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array)
        {
            var errorList = new List<string>();
            foreach (var err in errors.EnumerateArray())
            {
                if (err.ValueKind == JsonValueKind.String)
                {
                    var str = err.GetString();
                    if (!string.IsNullOrEmpty(str))
                        errorList.Add(str);
                }
            }

            if (errorList.Count > 0)
            {
                message = string.Join("; ", errorList);
                return true;
            }
        }

        return false;
    }

    private string GetErrorMessage(HttpStatusCode statusCode, string? extractedErrorMessage, string? reasonPhrase)
    {
        // Define default messages for each status code
        var defaultMessage = statusCode switch
        {
            HttpStatusCode.Unauthorized => "Authentication required. Please sign in again.",
            HttpStatusCode.Forbidden => "You don't have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.BadRequest => "Invalid request data.",
            HttpStatusCode.InternalServerError => "Server error occurred. Please try again later.",
            _ => "Request failed. Please try again."
        };

        // Return the most specific available message
        if (!string.IsNullOrWhiteSpace(extractedErrorMessage))
        {
            return extractedErrorMessage;
        }

        if (!string.IsNullOrWhiteSpace(reasonPhrase))
        {
            return reasonPhrase;
        }

        return defaultMessage;
    }

    private static bool IsDecryptionException(Exception ex)
    {
        return ex is CryptographicException ||
               ex is FormatException ||
               ex is ArgumentException ||
               ex is InvalidOperationException;
    }

    private JsonSerializerOptions CreateResilientJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Add resilient enum converters for common enums
        options.Converters.Add(new EnumConverter<LeaveApplicationCardStatus>());
        options.Converters.Add(new EnumConverter<LeaveApplicationListStatus>());
        options.Converters.Add(new EnumConverter<Leave_Status>());
        options.Converters.Add(new EnumConverter<LeaveTypeStatus>());
        options.Converters.Add(new EnumConverter<Gender>());
        options.Converters.Add(new EnumConverter<Employee_Type>());
        options.Converters.Add(new EnumConverter<EmployeesStatus>());
        options.Converters.Add(new EnumConverter<Employment_Type>());

        // Add nullable versions
        options.Converters.Add(new NullableEnumConverter<LeaveApplicationCardStatus>());
        options.Converters.Add(new NullableEnumConverter<LeaveApplicationListStatus>());
        options.Converters.Add(new NullableEnumConverter<Leave_Status>());
        options.Converters.Add(new NullableEnumConverter<Gender>());
        options.Converters.Add(new NullableEnumConverter<Employee_Type>());
        options.Converters.Add(new NullableEnumConverter<Employment_Type>());

        return options;
    }

}

