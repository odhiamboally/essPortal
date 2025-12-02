namespace ESSPortal.Application.Configuration;
public class SecuritySettings
{
    public SessionManagementSettings SessionManagement { get; set; } = new();
    public PayloadEncryptionSettings PayloadEncryption { get; set; } = new();
}
