namespace ESSPortal.Application.Configuration;

public class BCSettings
{
    public string OdataBaseUrl { get; set; } = string.Empty;
    public string SoapServiceBaseUrl { get; set; } = string.Empty;
    public string OnlinePortalServicesEndpoint { get; set; } = string.Empty;


    // Template-based endpoints
    // Template for ESS endpoints - parameterized
    public string EmployeeSelfServiceEndpoint { get; set; } = "http://10.100.80.190:1073/BC200/ODataV4/{serviceName}?company=United%20Nations%20DT%20Sacco";


    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? CompanyName { get; set; }
    public int TimeoutSeconds { get; set; } = 100;
    public bool EnableRetry { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public bool UseHttps { get; set; } = true;
    public Dictionary<string, string> EntitySets { get; set; } = [];

}
