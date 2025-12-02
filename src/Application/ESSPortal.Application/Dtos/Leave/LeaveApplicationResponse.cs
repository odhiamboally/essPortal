namespace ESSPortal.Application.Dtos.Leave;
public record LeaveApplicationResponse
{
    public string? ReferenceNo { get; init; }
    public string ApplicationNo { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime SubmittedDate { get; init; } = DateTime.Now;
}
