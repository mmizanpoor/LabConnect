namespace LabConnectPortal.Api.Infrastructure.ViewModels.User;

public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int AdministratorCount { get; set; }
    public int LaboratoryCount { get; set; }
    public int UserCount { get; set; }
    public int StoreCount { get; set; }
    public int AdminLabCount { get; set; }
    public int MonthlySiteVisits { get; set; }
    public int OnlineUsersCount { get; set; }
}
