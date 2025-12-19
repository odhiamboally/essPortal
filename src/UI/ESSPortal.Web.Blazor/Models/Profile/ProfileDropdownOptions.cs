using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESSPortal.Web.Blazor.Models.Profile;

public class ProfileDropdownOptions
{
    public static List<SelectListItem> GenderOptions =>
    [
        new SelectListItem { Value = "", Text = "Select Gender" },
        new SelectListItem { Value = "Male", Text = "Male" },
        new SelectListItem { Value = "Female", Text = "Female" },
        new SelectListItem { Value = "Other", Text = "Other" },
        new SelectListItem { Value = "PreferNotToSay", Text = "Prefer not to say" }
    ];

    public static List<SelectListItem> CountryOptions =>
    [
        new SelectListItem { Value = "", Text = "Select Country" },
        new SelectListItem { Value = "KE", Text = "Kenya" },
        new SelectListItem { Value = "UG", Text = "Uganda" },
        new SelectListItem { Value = "TZ", Text = "Tanzania" },
        new SelectListItem { Value = "RW", Text = "Rwanda" },
        new SelectListItem { Value = "US", Text = "United States" },
        new SelectListItem { Value = "UK", Text = "United Kingdom" },
        new SelectListItem { Value = "CA", Text = "Canada" }
    ];

    public static List<SelectListItem> BankAccountTypeOptions =>
    [
        new SelectListItem { Value = "", Text = "Select Account Type" },
        new SelectListItem { Value = "Savings", Text = "Savings Account" },
        new SelectListItem { Value = "Current", Text = "Current Account" },
        new SelectListItem { Value = "Checking", Text = "Checking Account" }
    ];
}
