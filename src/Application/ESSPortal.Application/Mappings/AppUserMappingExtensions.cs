using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Dtos.Auth;
using ESSPortal.Domain.Entities;

namespace ESSPortal.Application.Mappings;
public static class AppUserMappingExtensions
{
    public static AppUser ToAppUser(this RegisterEmployeeRequest dto)
    {
        return new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmployeeNumber = dto.EmployeeNumber,
            FirstName = dto.FirstName ?? string.Empty,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender == Gender.Male ? "Male" :
                     dto.Gender == Gender.Female ? "Female" : "Other",
            Department = string.Empty, // Will be populated later from BC if needed
            JobTitle = string.Empty, // Will be populated later from BC if needed
            ManagerId = null, // Will be populated later from BC if needed
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = dto.IsActive,
            IsDeleted = dto.IsDeleted,
            RequirePasswordChange = false,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            FailedLoginAttempts = 0
        };

    }

    public static RegisterEmployeeResponse ToRegisterEmployeeResponse(this AppUser entity)
    {
        return new RegisterEmployeeResponse(
            entity.Id,
            true,
            string.Empty, // Confirmation link should be generated elsewhere
            string.Empty // Token should be generated elsewhere

        );
    }
}
