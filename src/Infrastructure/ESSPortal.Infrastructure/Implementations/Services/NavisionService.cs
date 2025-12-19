using ESSPortal.Application.Configuration;
using ESSPortal.Application.Contracts.Interfaces.Common;
using ESSPortal.Application.Contracts.Interfaces.Services;
using ESSPortal.Application.Dtos.Common;
using ESSPortal.Infrastructure.Utilities;

using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace ESSPortal.Infrastructure.Implementations.Services;
internal sealed class NavisionService : INavisionService
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _essHttpClient;
    private readonly ISoapService _soapService;
    private readonly INavisionUrlHelper _urlHelper;
    private readonly BCSettings _bCSettings;

    public NavisionService(
        IHttpClientFactory httpClientFactory, 
        ISoapService soapService, 
        INavisionUrlHelper urlHelper,
        IOptions<BCSettings> bCSettings)
    {
        //var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}"));
        

        _httpClient = httpClientFactory.CreateClient("NavisionService");
        _essHttpClient = httpClientFactory.CreateClient("NavisionService.Ess");

        //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        _soapService = soapService ?? throw new ArgumentNullException(nameof(soapService));
        _urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        _bCSettings = bCSettings.Value ?? throw new ArgumentNullException(nameof(_bCSettings));

    }


    public async Task<ApiResponse<(T, string RawJson)>> CreateAsync<T>(string serviceName, T entity) where T : class, new()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(serviceName, entity);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<T>(responseJson);

            return result != null
                ? ApiResponse<(T, string RawJson)>.Success("Success", (result, responseJson))
                : ApiResponse<(T, string RawJson)>.Failure("Failed to create entity.");
        }
        catch (Exception)
        {

            throw;
        }

    }

    public async Task<ApiResponse<(T, string RawJson)>> CreateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new()
    {
        try
        {
            var url = GetServiceUrl(serviceName);
            var response = await _essHttpClient.PostAsJsonAsync(url, entity);
            if(!response.IsSuccessStatusCode) 
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var errorResult = JsonSerializer.Deserialize<T>(errorJson);
                return ApiResponse<(T, string RawJson)>.Failure($"Failed to create entity: {response} {errorJson}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(responseJson);

            return result != null
                ? ApiResponse<(T, string RawJson)>.Success("Success", (result, responseJson))
                : ApiResponse<(T, string RawJson)>.Failure("Failed to create entity.");
        }
        catch (Exception)
        {

            throw;
        }

    }

    public async Task<ApiResponse<(T, string RawJson)>> UpdateLeaveApplicationAsync<T>(string serviceName, T entity) where T : class, new()
    {
        try
        {
            var url = GetServiceUrl(serviceName);
            var response = await _essHttpClient.PostAsJsonAsync(url, entity);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<T>(responseJson);

            return result != null
                ? ApiResponse<(T, string RawJson)>.Success("Success", (result, responseJson))
                : ApiResponse<(T, string RawJson)>.Failure("Failed to create entity.");
        }
        catch (Exception)
        {

            throw;
        }

    }

    public async Task<ApiResponse<List<T>>> CreateMultipleAsync<T>(string serviceName, List<T> entities) where T : class, new()
    {
        try
        {
            
            var response = await _httpClient.PostAsJsonAsync(serviceName, entities);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<T>>(responseJson);

            if (result is null || !result.Any())
            {
                return ApiResponse<List<T>>.Failure("Failed to create entities or no entities returned.");
            }

            //ToDo: Assuming the API returns the created entities, otherwise you can return a success message without the entities

            return result != null
                ? ApiResponse<List<T>>.Success("Success", result)
                : ApiResponse<List<T>>.Failure("Failed to create entities.");

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ApiResponse<T>> GetSingleAsync_<T>(string requestUri) where T : class, new()
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();

            // Since you know it's always OData format, deserialize directly
            var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
            var result = odataResponse?.Items?.FirstOrDefault();

            if (result == null)
            {
                return ApiResponse<T>.Failure("Entity not found or failed to deserialize.");
            }

            return ApiResponse<T>.Success("Entity fetched successfully.", result);
        }
        catch (JsonException ex)
        {
            return ApiResponse<T>.Failure($"JSON deserialization failed: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> GetSingleAsync<T>(string requestUri) where T : class, new()
    {
        try
        {
            // FIXED: Use ResponseContentRead to buffer entire response
            using var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<T>.Failure($"Business Central API returned {response.StatusCode}: {errorContent}");
            }

            // Read into byte array first for reliability
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var responseJson = System.Text.Encoding.UTF8.GetString(contentBytes);

            // Since you know it's always OData format, deserialize directly
            var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
            var result = odataResponse?.Items?.FirstOrDefault();

            if (result == null)
            {
                return ApiResponse<T>.Failure("Entity not found or failed to deserialize.");
            }

            return ApiResponse<T>.Success("Entity fetched successfully.", result);
        }
        catch (JsonException ex)
        {
            return ApiResponse<T>.Failure($"JSON deserialization failed: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<T>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ApiResponse<T>.Failure($"Request to Business Central timed out");
        }
        catch (Exception ex) when (ex.Message.Contains("copying content to a stream"))
        {
            return ApiResponse<T>.Failure("Failed to read complete response from Business Central. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }


    public async Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsync_<T>(string requestUri) where T : class, new()
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();

            List<T> result = [];

            try
            {
                var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
                result = odataResponse?.Items ?? [];
            }
            catch (JsonException ex)
            {
                return ApiResponse<(List<T>, string)>.Failure($"Failed to deserialize response: {ex.Message}");
            }

            if (result.Count == 0)
            {
                return ApiResponse<(List<T>, string)>.Failure("No data found.");
            }

            return ApiResponse<(List<T>, string)>.Success("Data fetched successfully.", (result, responseJson));
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsync<T>(string requestUri) where T : class, new()
    {
        try
        {
            // FIXED: Use ResponseContentRead to buffer entire response before returning
            // This prevents "Error while copying content to a stream" failures
            using var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);

            // Check status code before reading content
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<(List<T>, string)>.Failure($"Business Central API returned {response.StatusCode}: {errorContent}");
            }

            // FIXED: Read entire response into byte array first (more reliable than streaming)
            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var responseJson = System.Text.Encoding.UTF8.GetString(contentBytes);

            List<T> result = [];

            try
            {
                var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
                result = odataResponse?.Items ?? [];
            }
            catch (JsonException ex)
            {
                return ApiResponse<(List<T>, string)>.Failure($"Failed to deserialize response: {ex.Message}");
            }

            if (result.Count == 0)
            {
                return ApiResponse<(List<T>, string)>.Failure("No data found.");
            }

            return ApiResponse<(List<T>, string)>.Success("Data fetched successfully.", (result, responseJson));
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Request to Business Central timed out for: {requestUri}");
        }
        catch (TaskCanceledException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Request was cancelled: {ex.Message}");
        }
        catch (Exception ex) when (ex.Message.Contains("copying content to a stream"))
        {
            return ApiResponse<(List<T>, string)>.Failure("Failed to read complete response from Business Central. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Unexpected error: {ex.Message}");
        }
    }


    // Simpler version if you know it's always OData format:
    public async Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsyncSimple_<T>(string requestUri) where T : class, new()
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();

            var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
            var result = odataResponse?.Items ?? [];

            if (result.Count == 0)
            {
                return ApiResponse<(List<T>, string)>.Failure("No data found.");
            }

            return ApiResponse<(List<T>, string)>.Success("Data fetched successfully.", (result, responseJson));
        }
        catch (JsonException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Failed to deserialize JSON: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<ApiResponse<(List<T>, string RawJson)>> GetMultipleAsyncSimple<T>(string requestUri) where T : class, new()
    {
        try
        {
            // FIXED: Buffer entire response
            using var response = await _httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<(List<T>, string)>.Failure($"Business Central API returned {response.StatusCode}: {errorContent}");
            }

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var responseJson = System.Text.Encoding.UTF8.GetString(contentBytes);

            var odataResponse = JsonSerializer.Deserialize<PagedResult<T>>(responseJson);
            var result = odataResponse?.Items ?? [];

            if (result.Count == 0)
            {
                return ApiResponse<(List<T>, string)>.Failure("No data found.");
            }

            return ApiResponse<(List<T>, string)>.Success("Data fetched successfully.", (result, responseJson));
        }
        catch (JsonException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Failed to deserialize JSON: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"HTTP request failed: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Request to Business Central timed out");
        }
        catch (Exception ex) when (ex.Message.Contains("copying content to a stream"))
        {
            return ApiResponse<(List<T>, string)>.Failure("Failed to read complete response from Business Central. Please try again.");
        }
        catch (Exception ex)
        {
            return ApiResponse<(List<T>, string)>.Failure($"Unexpected error: {ex.Message}");
        }
    }


    public async Task<ApiResponse<string>> GenerateP9Async(string employeeNo, int year)
    {
        return await _soapService.GenerateP9Async(employeeNo, year);
    }

    public async Task<ApiResponse<string>> GeneratePaySlipAsync(string employeeNo, DateTime period)
    {
        return await _soapService.GeneratePaySlipAsync(employeeNo, period);
    }

    public async Task<ApiResponse<bool>> PostAsync(string requestUri)
    {
        try
        {
            var response = await _httpClient.PostAsync(requestUri, null);
            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync();

            return ApiResponse<bool>.Success("Operation successful", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ApiResponse<bool>> PostAsync(string requestUri, Dictionary<string, string> parameters)
    {
        try
        {
            var response = await _httpClient.PostAsync(requestUri, null);
            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStringAsync();

            return ApiResponse<bool>.Success("Operation successful", true);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ApiResponse<bool>> PostAsync<T>(string serviceName, T filter) where T : class, new()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(serviceName, filter);
            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();
            return ApiResponse<bool>.Success("Operation successful", true);

        }
        catch (Exception)
        {

            throw;
        }
    }

    private string GetServiceUrl(string serviceKey)
    {
        if (_urlHelper.IsEssService(serviceKey))
        {
            // For ESS services, get the actual service name from EntitySets and build ESS URL
            //var serviceName = _bCSettings.EntitySets[serviceKey];
            var serviceName = serviceKey;
            return _urlHelper.GetEssUrl(serviceName);
        }
        else
        {
            // For regular OData services
            return _urlHelper.GetODataUrl(serviceKey);
        }
    }

}
