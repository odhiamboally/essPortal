using System.ComponentModel.DataAnnotations;

namespace ESSPortal.Web.Blazor.ViewModels.Profile;

public class BankingInformationViewModel
{
    [Display(Name = "Bank Account Type")]
    public string? BankAccountType { get; set; }

    [Display(Name = "Bank Code")]
    [StringLength(10, ErrorMessage = "Bank code cannot exceed 10 characters")]
    public string? KBABankCode { get; set; }

    [Display(Name = "Branch Code")]
    [StringLength(10, ErrorMessage = "Branch code cannot exceed 10 characters")]
    public string? KBABranchCode { get; set; }

    [Display(Name = "Bank Account Number")]
    [StringLength(20, ErrorMessage = "Bank account number cannot exceed 20 characters")]
    public string? BankAccountNo { get; set; }
}
