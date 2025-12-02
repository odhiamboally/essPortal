using System.Text.Json.Serialization;

namespace EssPortal.Web.Mvc.Dtos.Common;

public class PagedResult<T>
{
    [JsonPropertyName("@odata.context")]
    public string? Context { get; init; }

    [JsonPropertyName("@odata.count")]
    public int? Count { get; init; }

    [JsonPropertyName("@odata.nextLink")]
    public string? NextLink { get; init; }

    [JsonPropertyName("@odata.deltaLink")]
    public string? DeltaLink { get; init; }

    [JsonPropertyName("value")]
    public List<T> Items { get; init; } = new();

    public int TotalCount { get; set; }
    public int? Cursor { get; set; }
    public int? PreviousCursor => Cursor;
    public int? NextCursor { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public bool IsFirstPage { get; set; }
    public bool IsLastPage { get; set; }
    public int TotalPages { get; set; }

}
