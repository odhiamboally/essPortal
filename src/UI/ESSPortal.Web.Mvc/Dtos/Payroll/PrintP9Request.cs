namespace ESSPortal.Web.Mvc.Dtos.Payroll;

public record PrintP9Request
{
    public string EmployeeNo { get; init; } = string.Empty;
    public int Year { get; init; }

    public string? LogoBase64 { get; set; }

    public string EmployerName { get; init; } = "United Nations DT Sacco";
    public string EmployerAddress { get; init; } = string.Empty;
    public bool IncludeCertificationSection { get; init; } = true;
    public bool UseLandscapeOrientation { get; init; } = true;
}
