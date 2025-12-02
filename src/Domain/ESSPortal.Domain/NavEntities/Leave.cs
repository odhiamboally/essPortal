namespace ESSPortal.Domain.NavEntities;
public class Leave : NavBaseEntity
{
    public LeaveApplicationCard? LeaveApplicationCard { get; set; }
    public LeaveRelievers? LeaveRelievers { get; set; }
    public List<LeaveStatisticsFactbox>? LeaveStatisticsFactbox { get; set; }

    
}
