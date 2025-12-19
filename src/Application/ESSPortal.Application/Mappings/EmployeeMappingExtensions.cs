using EssPortal.Domain.Enums.NavEnums;

using ESSPortal.Application.Dtos.Employee;
using ESSPortal.Domain.NavEntities;

using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Mappings;

public static class EmployeeMappingExtensions
{
    public static EmployeeResponse ToEmployeeResponse(this Employees emp) 
    {
        return new EmployeeResponse
        {
            Key = emp.No,
            No = emp.No,
            FullName = emp.FullName,
            FirstName = emp.First_Name,
            MiddleName = emp.Middle_Name,
            LastName = emp.Last_Name,
            JobPositionTitle = emp.Job_Position_Title,
            Initials = emp.Initials,
            JobTitle = emp.Job_Title,
            PostCode = emp.Post_Code,
            CountryRegionCode = emp.Country_Region_Code,
            PhoneNo = emp.Phone_No,
            Extension = emp.Extension,
            MobilePhoneNo = emp.Mobile_Phone_No,
            Email = emp.E_Mail,
            StatisticsGroupCode = emp.Statistics_Group_Code,
            ResourceNo = emp.Resource_No,

            PrivacyBlocked = emp.Privacy_Blocked ?? false,
            PrivacyBlockedSpecified = emp.Privacy_Blocked.HasValue,

            SearchName = emp.Search_Name,

            Comment = emp.Comment ?? false,
            CommentSpecified = emp.Comment.HasValue,

            Gender = Enum.TryParse<Gender>(emp.Gender, true, out var gender) ? gender : default,
            GenderSpecified = !string.IsNullOrWhiteSpace(emp.Gender),

            PINNumber = emp.PIN_Number,
            IdNo = emp.ID_No,
            SocialSecurityNo = emp.Social_Security_No,
            NHIFNo = emp.NHIF_No,

            Disabled = Enum.TryParse<Disabled>(emp.Disabled, true, out var disabled) ? disabled : default,
            DisabledSpecified = !string.IsNullOrWhiteSpace(emp.Disabled),

            EmploymentType = Enum.TryParse<Employment_Type>(emp.Employment_Type, true, out var employmentType) ? employmentType : default,
            EmploymentTypeSpecified = !string.IsNullOrWhiteSpace(emp.Employment_Type),

            Status = Enum.TryParse<EmployeesStatus>(emp.Status, true, out var status) ? status : default,
            StatusSpecified = !string.IsNullOrWhiteSpace(emp.Status),

            BankAccountNumber = emp.Bank_Account_Number
        };
    }

    public static EmployeeCardResponse ToEmployeeCardResponse(this EmployeeCard emp)
    {
        return new EmployeeCardResponse
        {
            No = emp.No,

            FirstName = emp.First_Name,
            MiddleName = emp.Middle_Name,
            LastName = emp.Last_Name,
            OtherName = emp.Other_Name,

            JobTitle = emp.Job_Title,
            JobPosition = emp.Job_Position,
            JobPositionTitle = emp.Job_Position_Title,
            SecondaryJobPosition = emp.Secondary_Job_Position,
            SecondaryJobPositionTitle = emp.Secondary_Job_Position_Title,
            ResponsibilityCenter = emp.Responsibility_Center,

            Initials = emp.Initials,
            SearchName = emp.Search_Name,
            Gender = emp.Gender,

            PhoneNo2 = emp.Phone_No_2,
            MobilePhoneNo = emp.Mobile_Phone_No,
            Extension = emp.Extension,
            Pager = emp.Pager,

            Email = emp.E_Mail,
            CompanyEmail = emp.Company_E_Mail,

            PrivacyBlocked = emp.Privacy_Blocked ?? false,
            PrivacyBlockedSpecified = emp.Privacy_Blocked.HasValue,

            UserId = emp.User_ID,
            ManagerSupervisor = emp.Manager_Supervisor,

            Address = emp.Address,
            Address2 = emp.Address_2,
            City = emp.City,
            County = emp.County,
            PostCode = emp.Post_Code,
            CountryRegionCode = emp.Country_Region_Code,

            EmploymentType = emp.Employment_Type,
            ContractType = emp.Contract_Type,
            ContractNumber = emp.Contract_Number,
            ContractStartDate = emp.Contract_Start_Date,
            ContractEndDate = emp.Contract_End_Date,

            EmploymentDate = emp.Employment_Date,
            BirthDate = emp.Birth_Date,

            Status = emp.Status,
            TerminationDate = emp.Termination_Date,

            IdNo = emp.ID_No,
            PinNumber = emp.PIN_Number,
            NhifNo = emp.NHIF_No,
            SocialSecurityNo = emp.Social_Security_No,

            BankAccountNumber = emp.Bank_Account_Number,
            BankAccountNo = emp.Bank_Account_No,
            BankBranch = emp.Bank_Branch,
            BankName = emp.Employee_Bank_Name,

            BasicPay = emp.Basic_Pay,
            HouseAllowance = emp.House_Allowance,
            TotalAllowances = emp.Total_Allowances,
            TotalDeductions = emp.Total_Deductions,

            LastDateModified = emp.Last_Date_Modified
        };
    }

    public static EmployeeCard ToEmployeeCard(this CreateEmployeeCardRequest req)
    {
        return new EmployeeCard
        {
            // Basic Information
            No = req.No,
            First_Name = req.First_Name,
            Middle_Name = req.Middle_Name,
            Last_Name = req.Last_Name,
            Other_Name = req.Other_Name,
            Job_Title = req.Job_Title,
            Initials = req.Initials,
            Search_Name = req.Search_Name,
            Gender = req.Gender,
            Phone_No_2 = req.Phone_No_2,
            Company_E_Mail = req.Company_E_Mail,
            Last_Date_Modified = req.Last_Date_Modified,
            Privacy_Blocked = req.Privacy_Blocked,
            User_ID = req.User_ID,
            Manager_Supervisor = req.Manager_Supervisor,
            Global_Dimension_1_Code = req.Global_Dimension_1_Code,
            Global_Dimension_2_Code = req.Global_Dimension_2_Code,
            Responsibility_Center = req.Responsibility_Center,

            // Address Information
            Address = req.Address,
            Address_2 = req.Address_2,
            City = req.City,
            County = req.County,
            Post_Code = req.Post_Code,
            Country_Region_Code = req.Country_Region_Code,
            ShowMap = req.ShowMap,

            // Contact Information
            Mobile_Phone_No = req.Mobile_Phone_No,
            Pager = req.Pager,
            Extension = req.Extension,
            E_Mail = req.E_Mail,
            Alt_Address_Code = req.Alt_Address_Code,
            Alt_Address_Start_Date = req.Alt_Address_Start_Date,
            Alt_Address_End_Date = req.Alt_Address_End_Date,

            // Job Information
            Job_Position = req.Job_Position,
            Job_Position_Title = req.Job_Position_Title,
            Secondary_Job_Position = req.Secondary_Job_Position,
            Secondary_Job_Position_Title = req.Secondary_Job_Position_Title,
            Employment_Type = req.Employment_Type,
            Contract_Type = req.Contract_Type,
            Contract_Number = req.Contract_Number,
            Contract_Length = req.Contract_Length,
            Contract_Start_Date = req.Contract_Start_Date,
            Contract_End_Date = req.Contract_End_Date,

            // Acting Information
            Acting_No = req.Acting_No,
            Control58 = req.Control58,
            Acting_Description = req.Acting_Description,
            Relieved_Employee = req.Relieved_Employee,
            Relieved_Name = req.Relieved_Name,
            Start_Date = req.Start_Date,
            End_Date = req.End_Date,
            Reason_for_Acting = req.Reason_for_Acting,

            // Employment Status
            Employment_Date = req.Employment_Date,
            Status = req.Status,
            Inactive_Date = req.Inactive_Date,
            Cause_of_Inactivity_Code = req.Cause_of_Inactivity_Code,
            InactiveDescription = req.InactiveDescription,
            Termination_Date = req.Termination_Date,
            Grounds_for_Term_Code = req.Grounds_for_Term_Code,
            Emplymt_Contract_Code = req.Emplymt_Contract_Code,
            Statistics_Group_Code = req.Statistics_Group_Code,
            Resource_No = req.Resource_No,
            Salespers_Purch_Code = req.Salespers_Purch_Code,

            // Personal Information
            Birth_Date = req.Birth_Date,
            Social_Security_No = req.Social_Security_No,
            Union_Code = req.Union_Code,
            Union_Membership_No = req.Union_Membership_No,
            Disabled = req.Disabled,
            Disability = req.Disability,
            Disability_Certificate = req.Disability_Certificate,
            Date_of_Birth_Age = req.Date_of_Birth_Age,
            ID_No = req.ID_No,
            Marital_Status = req.Marital_Status,
            Religion = req.Religion,
            Ethnic_Origin = req.Ethnic_Origin,
            Ethnic_Community = req.Ethnic_Community,
            Ethnic_Name = req.Ethnic_Name,
            Home_District = req.Home_District,
            First_Language = req.First_Language,
            Second_Language = req.Second_Language,
            Other_Language = req.Other_Language,

            // Financial Information
            Employee_Posting_Group = req.Employee_Posting_Group,
            Application_Method = req.Application_Method,
            Bank_Branch_No = req.Bank_Branch_No,
            Bank_Account_No = req.Bank_Account_No,
            IBAN = req.IBAN,
            SWIFT_Code = req.SWIFT_Code,
            BOSA_Member_No = req.BOSA_Member_No,
            FOSA_Account_No = req.FOSA_Account_No,
            PIN_Number = req.PIN_Number,
            NHIF_No = req.NHIF_No,
            Pay_Mode = req.Pay_Mode,
            Employee_x0027_s_Bank = req.Employee_x0027_s_Bank,
            Employee_Bank_Name = req.Employee_Bank_Name,
            Bank_Branch = req.Bank_Branch,
            Employee_Branch_Name = req.Employee_Branch_Name,
            Employee_Bank_Sort_Code = req.Employee_Bank_Sort_Code,
            Bank_Account_Number = req.Bank_Account_Number,
            Posting_Group = req.Posting_Group,
            Gratuity_Vendor_No = req.Gratuity_Vendor_No,
            Debtor_Code = req.Debtor_Code,
            Employee_Type = req.Employee_Type,
            Exempt_from_one_third_rule = req.Exempt_from_one_third_rule,

            // Salary Information
            Salary_Scale = req.Salary_Scale,
            Present = req.Present,
            Previous_Salary_Scale = req.Previous_Salary_Scale,
            Previous = req.Previous,
            Halt = req.Halt,
            Pays_tax_x003F_ = req.Pays_tax_x003F_,
            Secondary_Employee = req.Secondary_Employee,
            Insurance_Relief = req.Insurance_Relief,
            Pro_Rata_Calculated = req.Pro_Rata_Calculated,
            CurrBasicPay = req.CurrBasicPay,
            Basic_Pay = req.Basic_Pay,
            House_Allowance = req.House_Allowance,
            Insurance_Premium = req.Insurance_Premium,
            Total_Allowances = req.Total_Allowances,
            Total_Deductions = req.Total_Deductions,
            Taxable_Allowance = req.Taxable_Allowance,
            Cumm_PAYE = req.Cumm_PAYE,

            // Employment Dates & Periods
            Date_Of_Join = req.Date_Of_Join,
            Employment_Date_Age = req.Employment_Date_Age,
            Probation_Period = req.Probation_Period,
            End_Of_Probation_Date = req.End_Of_Probation_Date,
            Pension_Scheme_Join = req.Pension_Scheme_Join,
            Medical_Scheme_Join = req.Medical_Scheme_Join,
            Retirement_Date = req.Retirement_Date,
            Notice_Period = req.Notice_Period,
            Send_Alert_to = req.Send_Alert_to,
            Served_Notice_Period = req.Served_Notice_Period,
            Date_Of_Leaving = req.Date_Of_Leaving,
            Termination_Category = req.Termination_Category,
            Exit_Interview_Date = req.Exit_Interview_Date,
            Exit_Interview_Done_by = req.Exit_Interview_Done_by,
            Allow_Re_Employment_In_Future = req.Allow_Re_Employment_In_Future,

            // Increment Information
            Incremental_Month = req.Incremental_Month,
            Last_Increment_Date = req.Last_Increment_Date,
            Next_Increment_Date = req.Next_Increment_Date,
            Last_Date_Increment = req.Last_Date_Increment,
            Next_Date_Increment = req.Next_Date_Increment,
            Pay_Period_Filter = req.Pay_Period_Filter
        };
    }

}
