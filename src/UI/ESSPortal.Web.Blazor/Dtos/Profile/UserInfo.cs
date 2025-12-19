public record UserInfo(
    string UserId,
    string IDNo,
    string? EmployeeNumber,
    string? FirstName,
    string? LastName,
    string? Email,
    string? PhoneNumber,
    string ResponsibilityCenter,
    string JobPositionTitle,
    string ManagerSupervisor,
    string EmploymentType,
    string? ProfilePictureUrl,
    string? CountryRegionCode,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    bool TwoFactorEnabled,
    DateTimeOffset? LastLoginAt,
    List<string> Roles
);




//{

//    public UserInfo() : this(
//        string.Empty, 
//        string.Empty, 
//        null, 
//        null, 
//        null, 
//        null, 
//        null, 
//        string.Empty, 
//        string.Empty, 
//        string.Empty, 
//        string.Empty, 
//        null, 
//        null, 
//        false, 
//        false, 
//        false, 
//        null, 
//        new List<string>())
//    {
//    }
//};
