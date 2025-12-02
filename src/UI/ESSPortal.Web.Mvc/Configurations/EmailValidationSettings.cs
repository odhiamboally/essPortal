namespace ESSPortal.Web.Mvc.Configurations;

public class EmailValidationSettings
{
    public bool BlockPersonalDomains { get; set; } = true;
    public List<string> AllowedDomains { get; set; } = new();
    public List<string> BlockedDomains { get; set; } = new();
    public bool RequireBusinessEmail { get; set; } = true;
}
