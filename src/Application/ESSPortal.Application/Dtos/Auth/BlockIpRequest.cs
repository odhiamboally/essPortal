namespace ESSPortal.Application.Dtos.Auth;
public record BlockIpRequest(
    string IpAddress,
    string Reason,
    int? DurationMinutes = null
);
