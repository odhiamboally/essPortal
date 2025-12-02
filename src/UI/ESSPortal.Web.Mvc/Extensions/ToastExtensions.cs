using Microsoft.AspNetCore.Mvc;

namespace ESSPortal.Web.Mvc.Extensions;

public static class ToastExtensions
{
    private const string TOAST_SUCCESS_KEY = "__ToastSuccess";
    private const string TOAST_ERROR_KEY = "__ToastError";
    private const string TOAST_WARNING_KEY = "__ToastWarning";
    private const string TOAST_INFO_KEY = "__ToastInfo";

    public static void ToastSuccess(this Controller controller, string message, string? title = null)
    {
        controller.TempData[TOAST_SUCCESS_KEY] = message;
        if (!string.IsNullOrWhiteSpace(title))
        {
            controller.TempData[TOAST_SUCCESS_KEY + "_Title"] = title;
        }
    }

    public static void ToastError(this Controller controller, string message, string? title = null)
    {
        controller.TempData[TOAST_ERROR_KEY] = message;
        if (!string.IsNullOrWhiteSpace(title))
        {
            controller.TempData[TOAST_ERROR_KEY + "_Title"] = title;
        }
    }

    public static void ToastWarning(this Controller controller, string message, string? title = null)
    {
        controller.TempData[TOAST_WARNING_KEY] = message;
        if (!string.IsNullOrWhiteSpace(title))
        {
            controller.TempData[TOAST_WARNING_KEY + "_Title"] = title;
        }
    }

    public static void ToastInfo(this Controller controller, string message, string? title = null)
    {
        controller.TempData[TOAST_INFO_KEY] = message;
        if (!string.IsNullOrWhiteSpace(title))
        {
            controller.TempData[TOAST_INFO_KEY + "_Title"] = title;
        }
    }

    public static void ToastAuthSuccess(this Controller controller, string message, string? title = null)
    {
        // Only set auth messages if we're in auth context
        var actionName = controller.ControllerContext.ActionDescriptor.ActionName;
        var controllerName = controller.ControllerContext.ActionDescriptor.ControllerName;

        if (controllerName.Equals("Auth", StringComparison.OrdinalIgnoreCase) ||
            IsAuthRelatedAction(actionName))
        {
            controller.ToastSuccess(message, title ?? "Welcome");
        }
    }

    public static void ToastSessionInfo(this Controller controller, string message, string? title = null)
    {
        // Use different key for session-related messages
        controller.TempData["__ToastSession"] = message;
        if (!string.IsNullOrWhiteSpace(title))
        {
            controller.TempData["__ToastSession_Title"] = title;
        }
    }

    public static void ToastActivitySuccess(this Controller controller, string activity, string message)
    {
        var activityTitles = new Dictionary<string, string>
        {
            { "profile_update", "Profile Updated" },
            { "password_reset_requested", "Password Reset" },
            { "password_reset", "Password Reset Successful" },
            { "email_update", "Email Updated" },
            { "phone_update", "Phone Number Updated" },
            { "address_update", "Address Updated" },
            { "banking_info_update", "Banking Information Updated" },
            { "employment_details_update", "Employment Details Updated" },
            { "role_change", "Role Changed" },
            { "password_change", "Password Changed" },
            { "settings_update", "Settings Updated" },
            { "file_upload", "File Uploaded" },
            { "banking_update", "Banking Info Updated" },
            { "contact_update", "Contact Info Updated" },
            { "personal_update", "Personal Details Updated" },
            { "user_registered", "Employee Sign Up Successful" },
            { "user_invited", "Employee Invited" },
            { "user_deleted", "Employee Deleted" },
            { "user_suspended", "Employee Suspended" },
            { "user_activated", "Employee Activated" },
            { "email_confirmed", "Email Confirmed" },
            { "two_factor_enabled", "Two-Factor Authentication Enabled" },
            { "two_factor_disabled", "Two-Factor Authentication Disabled" },
            { "profile_picture_updated", "Profile Picture Updated" },
            { "account_locked", "Account Locked" },
            { "account_unlocked", "Account Unlocked" },

        };

        var title = activityTitles.GetValueOrDefault(activity, "Success");
        controller.ToastSuccess(message, title);
    }

    public static void ClearToastMessages(this Controller controller, params string[] types)
    {
        string[] allKeys =
        [
            TOAST_SUCCESS_KEY, TOAST_SUCCESS_KEY + "_Title",
            TOAST_ERROR_KEY, TOAST_ERROR_KEY + "_Title",
            TOAST_WARNING_KEY, TOAST_WARNING_KEY + "_Title",
            TOAST_INFO_KEY, TOAST_INFO_KEY + "_Title",
            "__ToastSession", "__ToastSession_Title"
        ];

        if (types.Length == 0)
        {
            // Clear all toast messages
            foreach (var key in allKeys)
            {
                controller.TempData.Remove(key);
            }
        }
        else
        {
            // Clear specific types
            foreach (var type in types)
            {
                var key = $"__Toast{char.ToUpperInvariant(type[0])}{type[1..]}";
                controller.TempData.Remove(key);
                controller.TempData.Remove(key + "_Title");
            }
        }
    }

    private static bool IsAuthRelatedAction(string actionName)
    {
        string[] authActions =
        [
            "SignIn", "SignOut", "Register", "ForgotPassword",
            "ResetPassword", "ConfirmEmail", "TwoFactorLogin"
        ];

        return authActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
    }
}

