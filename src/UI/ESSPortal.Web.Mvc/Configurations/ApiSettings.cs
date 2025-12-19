
namespace EssPortal.Web.Mvc.Configurations;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Endpoints ApiEndpoints { get; set; } = new();

    public class Endpoints
    {
        public AuthEndpoints Auth { get; set; } = new();
        public UserEndpoints User { get; set; } = new();
        public TwoFactorEndpoints TwoFactor { get; set; } = new();
        public PayrollEndpoints Payroll { get; set; } = new();
        public ProfileEndpoints Profile { get; set; } = new();
        public DashboardEndpoints Dashboard { get; set; } = new();
        public EmployeeEndpoints Employee { get; set; } = new();
        public LeaveEndpoints Leave { get; set; } = new();
        public LeaveApplicationCardEndpoints LeaveApplicationCard { get; set; } = new();
        public LeaveApplicationListEndpoints LeaveApplicationList { get; set; } = new();
        public LeavePeriodEndpoints LeavePeriod { get; set; } = new();
        public LeavePlannerLineEndpoints LeavePlannerLine { get; set; } = new();
        public LeaveRelieverEndpoints LeaveReliever { get; set; } = new();
        public LeaveStatisticsFactboxEndpoints LeaveStatisticsFactbox { get; set; } = new();
        public LeaveTypeEndpoints LeaveType { get; set; } = new();
        public PayrollPeriodEndpoints PayrollPeriod { get; set; } = new();


    }

    public class AuthEndpoints
    {
        public string RegisterEmployee { get; set; } = string.Empty;
        public string SendEmailConfirmation { get; set; } = string.Empty;
        public string ResendEmailConfirmation { get; set; } = string.Empty;
        public string ConfirmUserEmail { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string GetCurrentUser { get; set; } = string.Empty;
        public string Get2FAProviders { get; set; } = string.Empty;
        public string Send2FACode { get; set; } = string.Empty;
        public string Verify2FACode { get; set; } = string.Empty;
        public string ProcessExternalLogin { get; set; } = string.Empty;
        public string HandleExternalLoginCallback { get; set; } = string.Empty;
        public string LinkExternalLogin { get; set; } = string.Empty;
        public string RequestPasswordReset { get; set; } = string.Empty;
        public string ValidatePasswordResetToken { get; set; } = string.Empty;
        public string ResetPassword { get; set; } = string.Empty;
        public string VerifyPassword { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string SignOut { get; set; } = string.Empty;
        public string SessionKeepAlive { get; set; } = string.Empty;
        public string SessionUnlock { get; set; } = string.Empty;
    }

    public class UserEndpoints
    {
        public string GetUserIdFromEmployeeNumber { get; set; } = string.Empty;
    }

    public class TwoFactorEndpoints
    {
        public string GetSetupInfo { get; set; } = string.Empty;
        public string Enable { get; set; } = string.Empty;
        public string Disable { get; set; } = string.Empty;
        public string GetStatus { get; set; } = string.Empty;
        public string GenerateBackupCodes { get; set; } = string.Empty;
        public string VerifyTotpCode { get; set; } = string.Empty;
    }

    public class PayrollEndpoints
    {
        public string GenerateP9 { get; set; } = string.Empty;
        public string GeneratePayslip { get; set; } = string.Empty;

    }

    public class ProfileEndpoints
    {
        public string GetUserProfile { get; set; } = string.Empty;
        public string UpdatePersonalDetails { get; set; } = string.Empty;
        public string UpdateContactInfo { get; set; } = string.Empty;
        public string UpdateBankingInfo { get; set; } = string.Empty;
        public string UpdateProfilePicture { get; set; } = string.Empty;
        public string ValidateProfileData { get; set; } = string.Empty;
        public string CalculateProfileCompletion { get; set; } = string.Empty;
    }

    public class DashboardEndpoints
    {
        public string GetDashboardData { get; set; } = string.Empty;
    }

    public class EmployeeEndpoints
    {
        public string Employees { get; set; } = string.Empty;
        public string EmployeeByNo { get; set; } = string.Empty;
        public string EmployeeByRecId { get; set; } = string.Empty;
        public string SearchEmployees { get; set; } = string.Empty;
        public string EmployeeCards { get; set; } = string.Empty;
        public string EmployeeCardByNo { get; set; } = string.Empty;
        public string EmployeeCardByRecId { get; set; } = string.Empty;
        public string SearchEmployeeCards { get; set; } = string.Empty;
        public string EmployeePhoto { get; set; } = string.Empty;
        public string RecIdFromKey { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string CheckEmployeeNumber { get; set; } = string.Empty;

    }

    public class LeaveEndpoints
    {
        public string CreateLeaveApplication { get; set; } = string.Empty;
        public string EditLeaveApplication { get; set; } = string.Empty;
        public string GetEmployeeLeaveBalance { get; set; } = string.Empty;
        public string GetEmployeeLeaveBalances { get; set; } = string.Empty;
        public string ManageLeave { get; set; } = string.Empty;
        public string GetLeaveDetails { get; set; } = string.Empty;
        public string GetLeaveHistory { get; set; } = string.Empty;
        public string GetLeaveRelieversByApplicationNo { get; set; } = string.Empty;
        public string GetLeaveApplicationCards { get; set; } = string.Empty;
        public string GetLeaveApplicationLists { get; set; } = string.Empty;
        public string GetLeaveTypes { get; set; } = string.Empty;
        public string GetLeavePeriods { get; set; } = string.Empty;
        public string GetLeaveStatistics { get; set; } = string.Empty;
        public string GetAnnualLeaveSummary { get; set; } = string.Empty; 
        public string GetLeaveSummary { get; set; } = string.Empty; 
    }

    public class LeaveApplicationCardEndpoints
    {
        public string GetLeaveApplicationCards { get; set; } = string.Empty;
        public string GetLeaveApplicationCardByNo { get; set; } = string.Empty;
        public string GetLeaveApplicationCardByRecId { get; set; } = string.Empty;
        public string SearchLeaveApplicationCards { get; set; } = string.Empty;
        public string CreateLeaveApplicationCard { get; set; } = string.Empty;
        public string CreateMultipleLeaveApplicationCards { get; set; } = string.Empty;
        public string EditLeaveApplicationCard { get; set; } = string.Empty;
        public string UpdateMultipleLeaveApplicationCards { get; set; } = string.Empty;
        public string DeleteLeaveApplicationCard { get; set; } = string.Empty;
        public string GetLeaveApplicationCardRecIdFromKey { get; set; } = string.Empty;
        public string IsLeaveApplicationCardUpdated { get; set; } = string.Empty;
    }

    public class LeaveApplicationListEndpoints
    {
        public string GetLeaveApplicationLists { get; set; } = string.Empty;
        public string GetLeaveApplicationListByNo { get; set; } = string.Empty;
        public string GetLeaveApplicationListByRecId { get; set; } = string.Empty;
        public string SearchLeaveApplicationLists { get; set; } = string.Empty;
        public string CreateLeaveApplicationList { get; set; } = string.Empty;
        public string CreateMultipleLeaveApplicationLists { get; set; } = string.Empty;
        public string EditLeaveApplicationList { get; set; } = string.Empty;
        public string UpdateMultipleLeaveApplicationLists { get; set; } = string.Empty;
        public string DeleteLeaveApplicationList { get; set; } = string.Empty;
        public string GetLeaveApplicationListRecIdFromKey { get; set; } = string.Empty;
        public string IsLeaveApplicationListUpdated { get; set; } = string.Empty;
    }

    public class LeavePeriodEndpoints
    {
        public string GetLeavePeriods { get; set; } = string.Empty;
        public string GetLeavePeriodByCode { get; set; } = string.Empty;
        public string GetLeavePeriodByRecId { get; set; } = string.Empty;
        public string SearchLeavePeriods { get; set; } = string.Empty;
        public string CreateLeavePeriod { get; set; } = string.Empty;
        public string CreateMultipleLeavePeriods { get; set; } = string.Empty;
        public string UpdateLeavePeriod { get; set; } = string.Empty;
        public string UpdateMultipleLeavePeriods { get; set; } = string.Empty;
        public string DeleteLeavePeriod { get; set; } = string.Empty;
        public string GetLeavePeriodRecIdFromKey { get; set; } = string.Empty;
        public string IsLeavePeriodUpdated { get; set; } = string.Empty;
    }

    public class LeavePlannerLineEndpoints
    {
        public string GetLeavePlannerLines { get; set; } = string.Empty;
        public string GetLeavePlannerLineByComposite { get; set; } = string.Empty;
        public string GetLeavePlannerLineByRecId { get; set; } = string.Empty;
        public string SearchLeavePlannerLines { get; set; } = string.Empty;
        public string CreateLeavePlannerLine { get; set; } = string.Empty;
        public string CreateMultipleLeavePlannerLines { get; set; } = string.Empty;
        public string UpdateLeavePlannerLine { get; set; } = string.Empty;
        public string UpdateMultipleLeavePlannerLines { get; set; } = string.Empty;
        public string DeleteLeavePlannerLine { get; set; } = string.Empty;
        public string GetLeavePlannerLineRecIdFromKey { get; set; } = string.Empty;
        public string IsLeavePlannerLineUpdated { get; set; } = string.Empty;
    }

    public class LeaveRelieverEndpoints
    {
        public string GetLeaveRelievers { get; set; } = string.Empty;
        public string GetLeaveRelieverByComposite { get; set; } = string.Empty;
        public string GetLeaveRelieverByRecId { get; set; } = string.Empty;
        public string GetLeaveRelieversByApplicationNo { get; set; } = string.Empty;
        public string SearchLeaveRelievers { get; set; } = string.Empty;
        public string CreateLeaveReliever { get; set; } = string.Empty;
        public string CreateMultipleLeaveRelievers { get; set; } = string.Empty;
        public string UpdateLeaveReliever { get; set; } = string.Empty;
        public string UpdateMultipleLeaveRelievers { get; set; } = string.Empty;
        public string DeleteLeaveReliever { get; set; } = string.Empty;
        public string GetLeaveRelieverRecIdFromKey { get; set; } = string.Empty;
        public string IsLeaveRelieverUpdated { get; set; } = string.Empty;
    }

    public class LeaveStatisticsFactboxEndpoints
    {
        public string GetLeaveStatistics { get; set; } = string.Empty;
        public string GetLeaveStatsByRecId { get; set; } = string.Empty;
        public string SearchLeaveStatistics { get; set; } = string.Empty;
        public string GetLeaveStatsRecIdFromKey { get; set; } = string.Empty;
        public string IsLeaveStatsUpdated { get; set; } = string.Empty;
    }

    public class LeaveTypeEndpoints
    {
        public string GetLeaveTypes { get; set; } = string.Empty;
        public string GetLeaveTypeByCode { get; set; } = string.Empty;
        public string GetLeaveTypeByRecId { get; set; } = string.Empty;
        public string SearchLeaveTypes { get; set; } = string.Empty;
        public string CreateLeaveType { get; set; } = string.Empty;
        public string CreateMultipleLeaveTypes { get; set; } = string.Empty;
        public string UpdateLeaveType { get; set; } = string.Empty;
        public string UpdateMultipleLeaveTypes { get; set; } = string.Empty;
        public string DeleteLeaveType { get; set; } = string.Empty;
        public string GetLeaveTypeRecIdFromKey { get; set; } = string.Empty;
        public string IsLeaveTypeUpdated { get; set; } = string.Empty;
    }
    
    public class PayrollPeriodEndpoints
    {
        public string GetPayrollPeriods { get; set; } = string.Empty;
        public string GetPayrollPeriodByStartingDate { get; set; } = string.Empty;
        public string GetPayrollPeriodByRecId { get; set; } = string.Empty;
        public string SearchPayrollPeriods { get; set; } = string.Empty;
        public string CreatePayrollPeriod { get; set; } = string.Empty;
        public string CreateMultiplePayrollPeriods { get; set; } = string.Empty;
        public string UpdatePayrollPeriod { get; set; } = string.Empty;
        public string UpdateMultiplePayrollPeriods { get; set; } = string.Empty;
        public string DeletePayrollPeriod { get; set; } = string.Empty;
        public string GetPayrollPeriodRecIdFromKey { get; set; } = string.Empty;
        public string IsPayrollPeriodUpdated { get; set; } = string.Empty;
    }
}
