namespace ESSPortal.Application.Dtos.Profile;
public record ProfileResponse(
    string EmpNo,
    string Email,
    string Name,
    string No,
    string PhysicalAddress,
    string PostalAddress,
    string PostCode,
    string TelephoneNo,
    string MobileNo,
    string City,
    string BankAccountType,
    string KBABankCode,
    string KBABranchCode,
    string BankAccountNo
);