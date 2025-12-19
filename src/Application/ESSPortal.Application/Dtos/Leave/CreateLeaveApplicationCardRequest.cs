using System;
using System.Collections.Generic;
using System.Text;

namespace ESSPortal.Application.Dtos.Leave;

public class CreateLeaveApplicationCardRequest
{
    public string Application_No { get; set; } = string.Empty;
    public DateTime Application_Date { get; set; }
    public bool? Apply_on_behalf { get; set; }

    public string? Employee_No { get; set; }
    public string? Employee_Name { get; set; }
    public string? Employment_Type { get; set; }
    public string? Responsibility_Center { get; set; }
    public string? Mobile_No { get; set; }

    public string? Shortcut_Dimension_1_Code { get; set; }
    public string? Shortcut_Dimension_2_Code { get; set; }

    public string? Leave_Period { get; set; }
    public string? Leave_Code { get; set; }
    public string? Leave_Status { get; set; }
    public string? Status { get; set; }

    public decimal? Leave_Earned_to_Date { get; set; }
    public decimal Days_Applied { get; set; }

    public DateTime Start_Date { get; set; }
    public DateTime End_Date { get; set; }
    public DateTime Resumption_Date { get; set; }

    public string? Duties_Taken_Over_By { get; set; }
    public string? Relieving_Name { get; set; }

    public bool? Leave_Allowance_Payable { get; set; }
    public string? Email_Adress { get; set; }
}
