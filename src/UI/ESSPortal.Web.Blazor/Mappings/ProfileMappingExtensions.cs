using ESSPortal.Application.Dtos.Profile;
using ESSPortal.Web.Blazor.Dtos.Profile;
using ESSPortal.Web.Blazor.ViewModels.Profile;

namespace ESSPortal.Web.Blazor.Mappings;

public static class ProfileMappingExtensions
{
    public static UserProfileViewModel ToUserProfileViewModel(this UserProfileResponse response)
    {
        return new UserProfileViewModel
        {
            UserId = response.UserId,
            EmployeeNumber = response.EmployeeNumber ?? string.Empty,
            Email = response.Email ?? string.Empty,
            UserName = response.Email ?? string.Empty, // or map from another field
            DisplayName = $"{response.FirstName} {response.LastName}".Trim(),
            ProfilePictureUrl = response.ProfilePictureUrl,
            LastLoginAt = response.LastLoginAt?.DateTime,
            CreatedAt = response.CreatedAt.DateTime,

            PersonalDetails = new PersonalDetailsViewModel
            {
                FirstName = response.FirstName ?? string.Empty,
                MiddleName = response.MiddleName,
                LastName = response.LastName ?? string.Empty,
                Gender = response.Gender
            },

            ContactInfo = new ContactInformationViewModel
            {
                CountryRegionCode = response.CountryRegionCode,
                PhysicalAddress = response.PhysicalAddress,
                TelephoneNo = response.TelephoneNo,
                MobileNo = response.MobileNo ?? string.Empty,
                PostalAddress = response.PostalAddress,
                PostCode = response.PostCode,
                City = response.City,
                ContactEMailAddress = response.ContactEMailAddress
            },

            BankingInfo = new BankingInformationViewModel
            {
                BankAccountType = response.BankAccountType,
                KBABankCode = response.KBABankCode,
                KBABranchCode = response.KBABranchCode,
                BankAccountNo = response.BankAccountNo
            },

            EmploymentDetails = new EmploymentDetailsViewModel
            {
                Department = response.Department,
                JobTitle = response.JobTitle,
                ManagerId = response.ManagerId,
                ManagerName = response.ManagerName
            }
        };
    }

}
