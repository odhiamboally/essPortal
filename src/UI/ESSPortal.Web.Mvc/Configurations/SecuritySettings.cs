namespace ESSPortal.Web.Mvc.Configurations;

public class SecuritySettings
{
    public SessionManagementSettings SessionManagement { get; set; } = new();
    public PayloadEncryptionSettings PayloadEncryption { get; set; } = new();
}
