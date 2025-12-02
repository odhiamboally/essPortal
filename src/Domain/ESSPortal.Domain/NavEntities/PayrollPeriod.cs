namespace ESSPortal.Domain.NavEntities;

public class PayrollPeriod
{
    public DateTime Starting_Date { get; set; }
    public string? Name { get; set; }
    public bool? New_Fiscal_Year { get; set; }
    public DateTime? Pay_Date { get; set; }
    public bool? Closed { get; set; }
    public bool? Close_Pay { get; set; }
    public decimal? P_A_Y_E { get; set; }
    public decimal? Basic_Pay { get; set; }
    public string? Approval_Status { get; set; }
}
