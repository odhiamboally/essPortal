namespace ESSPortal.Application.Dtos.Payroll;
public record PrintP9Request(
    string EmployeeNo,
    int Year,
    string? LogoBase64 = null,
    string EmployerName = "United Nations DT Sacco",
    string EmployerAddress = "",
    bool IncludeCertificationSection = true,
    bool UseLandscapeOrientation = true
    );

