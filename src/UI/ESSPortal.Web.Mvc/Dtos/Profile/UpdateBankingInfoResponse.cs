namespace ESSPortal.Web.Mvc.Dtos.Profile;

public record UpdateBankingInfoResponse(
    string UserId,
    string? BankAccountType,
    string? KBABankCode,
    string? KBABranchCode,
    string? BankAccountNo
);
