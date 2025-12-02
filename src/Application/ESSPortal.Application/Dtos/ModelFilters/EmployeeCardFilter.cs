using EssPortal.Domain.Enums.NavEnums;
using ESSPortal.Application.Dtos.ModelFilters;

namespace EssPortal.Application.Dtos.ModelFilters;

public record EmployeeCardFilter : BaseFilter
{
    public string? No { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? OtherName { get; init; }
    public string? JobTitle { get; init; }
    public string? Initials { get; init; }
    public string? SearchName { get; init; }
    public string? PhoneNo2 { get; init; }
    public string? CompanyEMail { get; init; }
    public string? LastDateModified { get; init; }
    public string? PrivacyBlocked { get; init; }
    public string? UserId { get; init; }
    public string? ManagerSupervisor { get; init; }
    public string? GlobalDimension1Code { get; init; }
    public string? GlobalDimension2Code { get; init; }
    public string? Responsibility_Center { get; init; }
    public string? Address { get; init; }
    public string? Address2 { get; init; }
    public string? City { get; init; }
    public string? County { get; init; }
    public string? PostCode { get; init; }
    public string? CountryRegionCode { get; init; }
    public string? ShowMap { get; init; }
    public string? MobilePhoneNo { get; init; }
    public string? Pager { get; init; }
    public string? Extension { get; init; }
    public string? EMail { get; init; }
    public string? AltAddressCode { get; init; }
    public string? AltAddressStartDate { get; init; }
    public string? AltAddressEndDate { get; init; }
    public string? JobPosition { get; init; }
    public string? JobPositionTitle { get; init; }
    public string? SecondaryJobPosition { get; init; }
    public string? SecondaryJobPositionTitle { get; init; }
    public string? ContractType { get; init; }
    public string? ContractNumber { get; init; }
    public string? ContractLength { get; init; }
    public string? ContractStartDate { get; init; }
    public string? ContractEndDate { get; init; }
    public string? ActingNo { get; init; }
    public string? Control58 { get; init; }
    public string? ActingDescription { get; init; }
    public string? RelievedEmployee { get; init; }
    public string? RelievedName { get; init; }
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? ReasonForActing { get; init; }
    public string? EmploymentDate { get; init; }
    public string? InactiveDate { get; init; }
    public string? CauseOfInactivityCode { get; init; }
    public string? InactiveDescription { get; init; }
    public string? TerminationDate { get; init; }
    public string? GroundsForTermCode { get; init; }
    public string? EmplymtContractCode { get; init; }
    public string? StatisticsGroupCode { get; init; }
    public string? ResourceNo { get; init; }
    public string? SalespersPurchCode { get; init; }
    public string? BirthDate { get; init; }
    public string? SocialSecurityNo { get; init; }
    public string? UnionCode { get; init; }
    public string? UnionMembershipNo { get; init; }
    public string? Disability { get; init; }
    public string? DisabilityCertificate { get; init; }
    public string? DateOfBirthAge { get; init; }
    public string? IdNo { get; init; }
    public string? Religion { get; init; }
    public string? EthnicCommunity { get; init; }
    public string? EthnicName { get; init; }
    public string? HomeDistrict { get; init; }
    public string? FirstLanguage { get; init; }
    public string? SecondLanguage { get; init; }
    public string? OtherLanguage { get; init; }
    public string? EmployeePostingGroup { get; init; }
    public string? BankBranchNo { get; init; }
    public string? BankAccountNo { get; init; }
    public string? Iban { get; init; }
    public string? SwiftCode { get; init; }
    public string? BosaMemberNo { get; init; }
    public string? FosaAccountNo { get; init; }
    public string? PinNumber { get; init; }
    public string? NhifNo { get; init; }
    public string? PayMode { get; init; }
    public string? EmployeesBank { get; init; }
    public string? EmployeeBankName { get; init; }
    public string? BankBranch { get; init; }
    public string? EmployeeBranchName { get; init; }
    public string? EmployeeBankSortCode { get; init; }
    public string? BankAccountNumber { get; init; }
    public string? PostingGroup { get; init; }
    public string? GratuityVendorNo { get; init; }
    public string? DebtorCode { get; init; }
    public string? SalaryScale { get; init; }
    public string? Present { get; init; }
    public string? PreviousSalaryScale { get; init; }
    public string? Previous { get; init; }
    public string? Halt { get; init; }
    public string? PaysTax { get; init; }
    public string? SecondaryEmployee { get; init; }
    public string? InsuranceRelief { get; init; }
    public string? ProRataCalculated { get; init; }
    public string? CurrBasicPay { get; init; }
    public string? BasicPay { get; init; }
    public string? HouseAllowance { get; init; }
    public string? InsurancePremium { get; init; }
    public string? TotalAllowances { get; init; }
    public string? TotalDeductions { get; init; }
    public string? TaxableAllowance { get; init; }
    public string? CummPaye { get; init; }
    public string? DateOfJoin { get; init; }
    public string? EmploymentDateAge { get; init; }
    public string? ProbationPeriod { get; init; }
    public string? EndOfProbationDate { get; init; }
    public string? PensionSchemeJoin { get; init; }
    public string? MedicalSchemeJoin { get; init; }
    public string? RetirementDate { get; init; }
    public string? NoticePeriod { get; init; }
    public string? SendAlertTo { get; init; }
    public string? ServedNoticePeriod { get; init; }
    public string? DateOfLeaving { get; init; }
    public string? ExitInterviewDate { get; init; }
    public string? ExitInterviewDoneBy { get; init; }
    public string? AllowReEmploymentInFuture { get; init; }
    public string? IncrementalMonth { get; init; }
    public string? LastIncrementDate { get; init; }
    public string? NextIncrementDate { get; init; }
    public Gender? Gender { get; init; }
    public Employee_Type? EmploymentType { get; init; }
    public EmployeesStatus? Status { get; init; }
    public EmployeeCardStatus? Disabled { get; init; }
    public Marital_Status? MaritalStatus { get; init; }
    public Ethnic_Origin? EthnicOrigin { get; init; }
    public Application_Method? ApplicationMethod { get; init; }
    public Employee_Type? EmployeeType { get; init; }
    public Termination_Category? TerminationCategory { get; init; }


    //public Dictionary<string, string?> CustomQueryParameters()
    //{
    //   var parameters = new Dictionary<string, string?>();

    //   void AddIf(string key, string? value)
    //   {
    //      if (!string.IsNullOrWhiteSpace(value))
    //         parameters[key] = value;
    //   }

    //   AddIf(nameof(No), No);
    //   AddIf(nameof(FirstName), FirstName);
    //   AddIf(nameof(MiddleName), MiddleName);
    //   AddIf(nameof(LastName), LastName);
    //   AddIf(nameof(OtherName), OtherName);
    //   AddIf(nameof(JobTitle), JobTitle);
    //   AddIf(nameof(Initials), Initials);
    //   AddIf(nameof(SearchName), SearchName);
    //   AddIf(nameof(Gender), Gender.ToString());
    //   AddIf(nameof(PhoneNo2), PhoneNo2);
    //   AddIf(nameof(CompanyEMail), CompanyEMail);
    //   AddIf(nameof(LastDateModified), LastDateModified);
    //   AddIf(nameof(PrivacyBlocked), PrivacyBlocked);
    //   AddIf(nameof(UserId), UserId);
    //   AddIf(nameof(ManagerSupervisor), ManagerSupervisor);
    //   AddIf(nameof(GlobalDimension1Code), GlobalDimension1Code);
    //   AddIf(nameof(GlobalDimension2Code), GlobalDimension2Code);
    //   AddIf(nameof(ResponsibilityCenter), ResponsibilityCenter);
    //   AddIf(nameof(Address), Address);
    //   AddIf(nameof(Address2), Address2);
    //   AddIf(nameof(City), City);
    //   AddIf(nameof(County), County);
    //   AddIf(nameof(PostCode), PostCode);
    //   AddIf(nameof(CountryRegionCode), CountryRegionCode);
    //   AddIf(nameof(ShowMap), ShowMap);
    //   AddIf(nameof(MobilePhoneNo), MobilePhoneNo);
    //   AddIf(nameof(Pager), Pager);
    //   AddIf(nameof(Extension), Extension);
    //   AddIf(nameof(EMail), EMail);
    //   AddIf(nameof(AltAddressCode), AltAddressCode);
    //   AddIf(nameof(AltAddressStartDate), AltAddressStartDate);
    //   AddIf(nameof(AltAddressEndDate), AltAddressEndDate);
    //   AddIf(nameof(JobPosition), JobPosition);
    //   AddIf(nameof(JobPositionTitle), JobPositionTitle);
    //   AddIf(nameof(SecondaryJobPosition), SecondaryJobPosition);
    //   AddIf(nameof(SecondaryJobPositionTitle), SecondaryJobPositionTitle);
    //   AddIf(nameof(EmploymentType), EmploymentType.ToString() );
    //   AddIf(nameof(ContractType), ContractType);
    //   AddIf(nameof(ContractNumber), ContractNumber);
    //   AddIf(nameof(ContractLength), ContractLength);
    //   AddIf(nameof(ContractStartDate), ContractStartDate);
    //   AddIf(nameof(ContractEndDate), ContractEndDate);
    //   AddIf(nameof(ActingNo), ActingNo);
    //   AddIf(nameof(Control58), Control58);
    //   AddIf(nameof(ActingDescription), ActingDescription);
    //   AddIf(nameof(RelievedEmployee), RelievedEmployee);
    //   AddIf(nameof(RelievedName), RelievedName);
    //   AddIf(nameof(StartDate), StartDate);
    //   AddIf(nameof(EndDate), EndDate);
    //   AddIf(nameof(ReasonForActing), ReasonForActing);
    //   AddIf(nameof(EmploymentDate), EmploymentDate);
    //   AddIf(nameof(Status), Status.ToString() );
    //   AddIf(nameof(InactiveDate), InactiveDate);
    //   AddIf(nameof(CauseOfInactivityCode), CauseOfInactivityCode);
    //   AddIf(nameof(InactiveDescription), InactiveDescription);
    //   AddIf(nameof(TerminationDate), TerminationDate);
    //   AddIf(nameof(GroundsForTermCode), GroundsForTermCode);
    //   AddIf(nameof(EmplymtContractCode), EmplymtContractCode);
    //   AddIf(nameof(StatisticsGroupCode), StatisticsGroupCode);
    //   AddIf(nameof(ResourceNo), ResourceNo);
    //   AddIf(nameof(SalespersPurchCode), SalespersPurchCode);
    //   AddIf(nameof(BirthDate), BirthDate);
    //   AddIf(nameof(SocialSecurityNo), SocialSecurityNo);
    //   AddIf(nameof(UnionCode), UnionCode);
    //   AddIf(nameof(UnionMembershipNo), UnionMembershipNo);
    //   AddIf(nameof(Disabled), Disabled.ToString());
    //   AddIf(nameof(Disability), Disability);
    //   AddIf(nameof(DisabilityCertificate), DisabilityCertificate);
    //   AddIf(nameof(DateOfBirthAge), DateOfBirthAge);
    //   AddIf(nameof(IdNo), IdNo);
    //   AddIf(nameof(MaritalStatus), MaritalStatus.ToString());
    //   AddIf(nameof(Religion), Religion);
    //   AddIf(nameof(EthnicOrigin), EthnicOrigin.ToString());
    //   AddIf(nameof(EthnicCommunity), EthnicCommunity);
    //   AddIf(nameof(EthnicName), EthnicName);
    //   AddIf(nameof(HomeDistrict), HomeDistrict);
    //   AddIf(nameof(FirstLanguage), FirstLanguage);
    //   AddIf(nameof(SecondLanguage), SecondLanguage);
    //   AddIf(nameof(OtherLanguage), OtherLanguage);
    //   AddIf(nameof(EmployeePostingGroup), EmployeePostingGroup);
    //   AddIf(nameof(ApplicationMethod), ApplicationMethod.ToString());
    //   AddIf(nameof(BankBranchNo), BankBranchNo);
    //   AddIf(nameof(BankAccountNo), BankAccountNo);
    //   AddIf(nameof(Iban), Iban);
    //   AddIf(nameof(SwiftCode), SwiftCode);
    //   AddIf(nameof(BosaMemberNo), BosaMemberNo);
    //   AddIf(nameof(FosaAccountNo), FosaAccountNo);
    //   AddIf(nameof(PinNumber), PinNumber);
    //   AddIf(nameof(NhifNo), NhifNo);
    //   AddIf(nameof(PayMode), PayMode);
    //   AddIf(nameof(EmployeesBank), EmployeesBank);
    //   AddIf(nameof(EmployeeBankName), EmployeeBankName);
    //   AddIf(nameof(BankBranch), BankBranch);
    //   AddIf(nameof(EmployeeBranchName), EmployeeBranchName);
    //   AddIf(nameof(EmployeeBankSortCode), EmployeeBankSortCode);
    //   AddIf(nameof(BankAccountNumber), BankAccountNumber);
    //   AddIf(nameof(PostingGroup), PostingGroup);
    //   AddIf(nameof(GratuityVendorNo), GratuityVendorNo);
    //   AddIf(nameof(DebtorCode), DebtorCode);
    //   AddIf(nameof(EmployeeType), EmployeeType.ToString());
    //   AddIf(nameof(SalaryScale), SalaryScale);
    //   AddIf(nameof(Present), Present);
    //   AddIf(nameof(PreviousSalaryScale), PreviousSalaryScale);
    //   AddIf(nameof(Previous), Previous);
    //   AddIf(nameof(Halt), Halt);
    //   AddIf(nameof(PaysTax), PaysTax);
    //   AddIf(nameof(SecondaryEmployee), SecondaryEmployee);
    //   AddIf(nameof(InsuranceRelief), InsuranceRelief);
    //   AddIf(nameof(ProRataCalculated), ProRataCalculated);
    //   AddIf(nameof(CurrBasicPay), CurrBasicPay);
    //   AddIf(nameof(BasicPay), BasicPay);
    //   AddIf(nameof(HouseAllowance), HouseAllowance);
    //   AddIf(nameof(InsurancePremium), InsurancePremium);
    //   AddIf(nameof(TotalAllowances), TotalAllowances);
    //   AddIf(nameof(TotalDeductions), TotalDeductions);
    //   AddIf(nameof(TaxableAllowance), TaxableAllowance);
    //   AddIf(nameof(CummPaye), CummPaye);
    //   AddIf(nameof(DateOfJoin), DateOfJoin);
    //   AddIf(nameof(EmploymentDateAge), EmploymentDateAge);
    //   AddIf(nameof(ProbationPeriod), ProbationPeriod);
    //   AddIf(nameof(EndOfProbationDate), EndOfProbationDate);
    //   AddIf(nameof(PensionSchemeJoin), PensionSchemeJoin);
    //   AddIf(nameof(MedicalSchemeJoin), MedicalSchemeJoin);
    //   AddIf(nameof(RetirementDate), RetirementDate);
    //   AddIf(nameof(NoticePeriod), NoticePeriod);
    //   AddIf(nameof(SendAlertTo), SendAlertTo);
    //   AddIf(nameof(ServedNoticePeriod), ServedNoticePeriod);
    //   AddIf(nameof(DateOfLeaving), DateOfLeaving);
    //   AddIf(nameof(TerminationCategory), TerminationCategory.ToString());
    //   AddIf(nameof(ExitInterviewDate), ExitInterviewDate);
    //   AddIf(nameof(ExitInterviewDoneBy), ExitInterviewDoneBy);
    //   AddIf(nameof(AllowReEmploymentInFuture), AllowReEmploymentInFuture);
    //   AddIf(nameof(IncrementalMonth), IncrementalMonth);
    //   AddIf(nameof(LastIncrementDate), LastIncrementDate);
    //   AddIf(nameof(NextIncrementDate), NextIncrementDate);

    //   return parameters;
    //}
}
