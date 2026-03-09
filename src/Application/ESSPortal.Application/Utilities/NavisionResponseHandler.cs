using ESSPortal.Shared.Dtos.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ESSPortal.Application.Utilities;
public static class NavisionResponseHandler
{
    public static Task<AppResponse<PagedResult<T>>> HandlePagedResponse<T>(AppResponse<(List<T> Items, string RawJson)> response)
    {
        if (!response.Successful)
        {
            return Task.FromResult(
                AppResponse<PagedResult<T>>.Failure(response.Message ?? "Failed to fetch records."));
        }

        try
        {
            var pagedResult = JsonSerializer.Deserialize<PagedResult<T>>(response.Data.RawJson);
            return Task.FromResult(
                pagedResult == null
                    ? AppResponse<PagedResult<T>>.Failure("Failed to deserialize response.")
                    : AppResponse<PagedResult<T>>.Success("Success", pagedResult));
        }
        catch (JsonException)
        {
            throw;
        }
    }
}
