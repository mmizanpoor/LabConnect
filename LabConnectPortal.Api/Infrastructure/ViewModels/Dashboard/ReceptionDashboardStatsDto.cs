namespace LabConnectPortal.Api.Infrastructure.ViewModels.Dashboard;

public class TopLabMetricDto
{
    public int? LabCode { get; set; }
    public string? LabName { get; set; }
    public int Count { get; set; }
}

public class LabReceptionSummaryRowDto
{
    public int LabCode { get; set; }
    public string? LabName { get; set; }
    public int ReceptionCount { get; set; }
    public int TestsCount { get; set; }
}

public class PopularTestMetricDto
{
    public string TestName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminReceptionDashboardStatsDto
{
    public TopLabMetricDto DailyMostUsage { get; set; } = new();
    public TopLabMetricDto MonthlyMostUsage { get; set; } = new();
    public TopLabMetricDto DailyMostSentTests { get; set; } = new();
    public TopLabMetricDto MonthlyMostSentTests { get; set; } = new();
    public TopLabMetricDto DailyMostReceivedTests { get; set; } = new();
    public TopLabMetricDto MonthlyMostReceivedTests { get; set; } = new();
    public List<LabReceptionSummaryRowDto> DailyLabs { get; set; } = [];
    public List<LabReceptionSummaryRowDto> MonthlyLabs { get; set; } = [];
    public LabReceptionPeriodStatsDto? SentDailyStats { get; set; }
    public LabReceptionPeriodStatsDto? SentMonthlyStats { get; set; }
    public LabReceptionPeriodStatsDto? ReceivedDailyStats { get; set; }
    public LabReceptionPeriodStatsDto? ReceivedMonthlyStats { get; set; }
    public List<LabReceptionSummaryRowDto> DailySentLabs { get; set; } = [];
    public List<LabReceptionSummaryRowDto> MonthlySentLabs { get; set; } = [];
    public List<LabReceptionSummaryRowDto> DailyReceivedLabs { get; set; } = [];
    public List<LabReceptionSummaryRowDto> MonthlyReceivedLabs { get; set; } = [];
    public List<PopularTestMetricDto> MonthlyPopularTests { get; set; } = [];
    public List<PopularTestMetricDto> YearlyPopularTests { get; set; } = [];
}

public class LabReceptionPeriodStatsDto
{
    public int UsageCount { get; set; }
    public int TestsCount { get; set; }
}

public class LabReceptionDirectionStatsDto
{
    public LabReceptionPeriodStatsDto Daily { get; set; } = new();
    public LabReceptionPeriodStatsDto Monthly { get; set; } = new();
    public List<LabReceptionSummaryRowDto> DailyPartners { get; set; } = [];
    public List<LabReceptionSummaryRowDto> MonthlyPartners { get; set; } = [];
}

public class LabReceptionDashboardStatsDto
{
    public LabReceptionDirectionStatsDto Sent { get; set; } = new();
    public LabReceptionDirectionStatsDto Received { get; set; } = new();
}
