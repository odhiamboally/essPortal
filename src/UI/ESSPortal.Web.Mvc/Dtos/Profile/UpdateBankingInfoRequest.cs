using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Mvc.Dtos.Profile;

public class UpdateBankingInfoRequest
{
    public string UserId { get; set; } = string.Empty;

    public string? BankAccountType { get; set; }

    [StringLength(10)]
    public string? KBABankCode { get; set; }

    [StringLength(10)]
    public string? KBABranchCode { get; set; }

    [StringLength(20)]
    public string? BankAccountNo { get; set; }
}
