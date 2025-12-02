namespace ESSPortal.Application.Dtos.Profile;
public record UpdateBankingInfoRequest(
    string UserId,
    string? BankAccountType,
    string? KBABankCode,
    string? KBABranchCode,
    string? BankAccountNo
);
