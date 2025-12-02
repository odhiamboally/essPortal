namespace ESSPortal.Application.Dtos.Auth;
public record SendEmailResponse(
    string MessageId,
    DateTimeOffset SentAt,
    string Recipient,
    string Subject
);

