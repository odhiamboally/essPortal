namespace ESSPortal.Web.Blazor.Dtos.Profile;

public record UpdateBankingInfoResponse(
    string UserId,
    string? BankAccountType,
    string? KBABankCode,
    string? KBABranchCode,
    string? BankAccountNo
);
