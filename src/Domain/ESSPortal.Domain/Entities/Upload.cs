using ESSPortal.Domain.Common;
using System.Text.Json.Serialization;

namespace ESSPortal.Domain.Entities;
public class Upload : BaseEntity
{
    public string? ProfileId { get; set; }
    public string? Path { get; set; }
    public string? FileType { get; set; }
    public string? FileName { get; set; }
    public string? UploadName { get; set; }
    public int? LineNo { get; set; }

    [JsonIgnore]
    public virtual Profile? Profile { get; set; }
}
