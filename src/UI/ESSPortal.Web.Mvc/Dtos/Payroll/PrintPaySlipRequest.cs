namespace ESSPortal.Web.Mvc.Dtos.Payroll;

public record PrintPaySlipRequest
{
    public string EmployeeNo { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Month { get; init; }

    public string? LogoBase64 { get; init; }

    // Additional company details
    public string CompanyName { get; init; } = "United Nations DT Sacco";
    public string CompanyAddress { get; init; } = "UN Sacco Building, United Nations Complex, UN Avenue";
    public string CompanyCity { get; init; } = "Village Market-Nairobi";
    public string CompanyPhone { get; init; } = "+254 709 115 800";
}
