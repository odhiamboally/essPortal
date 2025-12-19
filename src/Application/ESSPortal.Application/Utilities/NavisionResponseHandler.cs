using ESSPortal.Application.Dtos.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ESSPortal.Application.Utilities;
public static class NavisionResponseHandler
{
    public static Task<ApiResponse<PagedResult<T>>> HandlePagedResponse<T>(ApiResponse<(List<T> Items, string RawJson)> response)
    {
        if (!response.Successful)
        {
            return Task.FromResult(
                ApiResponse<PagedResult<T>>.Failure(response.Message ?? "Failed to fetch records."));
        }

        try
        {
            var pagedResult = JsonSerializer.Deserialize<PagedResult<T>>(response.Data.RawJson);
            return Task.FromResult(
                pagedResult == null
                    ? ApiResponse<PagedResult<T>>.Failure("Failed to deserialize response.")
                    : ApiResponse<PagedResult<T>>.Success("Success", pagedResult));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(
                ApiResponse<PagedResult<T>>.Failure($"Deserialization error: {ex.Message}"));
        }
    }
}
