using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Dashboard;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class ReceptionService(
    IReceptionRepository receptionRepository,
    ISRLabRepository srLabRepository,
    IUserRepository userRepository) : IReceptionService
{
    public async Task<OperationResult> PostReceptionGroupAsync(List<ReceptionViewModel> list)
    {
        var items = new List<ReceptionViewModel>();
        try
        {
            if (list == null)
                OperationResult.Failure("Reception List Is Empty");

            var receptionsNew = new List<ReceptionNew>();
            var receptTestNews = new List<ReceptTestNew>();
            var receptionKeys = new HashSet<(int SourceLabId, string SourceReceptId, int TargetLabId)>();

            var allReceptionKeys = list
                .SelectMany(x => x.ReceptTests)
                .Where(x => x.SourceLabId > 0 && x.TargetLabId > 0)
                .Select(x => new
                {
                    x.SourceLabId,
                    x.TargetLabId,
                    x.SourceReceptId
                })
                .Distinct()
                .Select(x => new ReceptionKeyViewModel
                {
                    SourceLabId = x.SourceLabId,
                    TargetLabId = x.TargetLabId,
                    SourceReceptId = x.SourceReceptId
                })
                .ToList();

            var receptions = await receptionRepository.GetReceptionsByKeys(allReceptionKeys);

            var receptionLookup = receptions?.ToDictionary(x => (
                                  x.intSourceLabId,
                                  x.chrSourceReceptId.Trim(),
                                  x.intTargetLabId
                                 ));

            foreach (var reception in list)
            {
                foreach (var receptTest in reception.ReceptTests)
                {
                    if (receptTest == null || receptTest.SourceLabId == 0 || receptTest.TargetLabId == 0) continue;

                    var key = (reception.SourceLabId, reception.SourceReceptId, receptTest.TargetLabId);

                    receptionLookup!.TryGetValue(key, out var activeReception);

                    if (activeReception == null && receptionKeys.Add(key))
                    {
                        receptionsNew.Add(new ReceptionNew(
                            reception.SourceLabId,
                            reception.SourceReceptId,
                            receptTest.TargetLabId,
                            reception.SourceSendReceptDate!.TrimEnd(),
                            reception.Age,
                            reception.AgeType,
                            reception.FirstName,
                            reception.LastName,
                            reception.Gender,
                            reception.PreviousRecords,
                            reception.IsUrgent,
                            reception.NIC,
                            reception.Mobile,
                            reception.DoctorCode
                        ));
                    }

                    var sourceCpn = receptTest.SourceCPN?.Trim();
                    var sourceTestName = receptTest.SourceTestName?.Trim();

                    bool testExistsInReception =
                        activeReception?.ReceptTests?.Any(x =>
                            x.vchSourceCPN?.Trim() == sourceCpn ||
                            x.vchSourceTestName?.Trim() == sourceTestName) == true;

                    bool shouldAddTest = activeReception == null || !testExistsInReception;

                    if (!shouldAddTest)
                        continue;

                    receptTestNews.Add(new ReceptTestNew
                    {
                        intSourceLabId = receptTest.SourceLabId,
                        chrSourceReceptId = receptTest.SourceReceptId,
                        chrSourceSendDate = receptTest.SourceSendDate ?? "",
                        intTargetLabId = receptTest.TargetLabId,
                        vchSourceCPN = receptTest.SourceCPN,
                        vchSourceTestName = receptTest?.SourceTestName?.Trim(),
                        bitEmg = receptTest.IsUrgent,
                        SourceApprovePrice = receptTest.SourceApprovePrice,
                        SourceTestId = receptTest.SourceTestId,
                        SourceSectionName = receptTest.SourceSectionName,
                        Tracking = receptTest.Tracking
                    });
                }
            }

            if (receptionsNew.Count > 0 && receptTestNews.Count > 0)
                receptionRepository.AddRangeReceptions(receptionsNew);
            if (receptTestNews.Count > 0)
                receptionRepository.AddRangeReceptTestNew(receptTestNews);

            var operationResult = await receptionRepository.SaveChangesAsync();
            if (operationResult.Success)
            {
                var finalResults = await receptionRepository.GetReceptionsByKeys(allReceptionKeys);
                if (finalResults != null && finalResults.Count > 0)
                    items = ReceptionModelConverter.ConvertModel(finalResults);
            }
            else
                return operationResult;

        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex.Message);
        }

        return OperationResult<IReadOnlyList<ReceptionViewModel>>.Success(items);
    }

    public Task<OperationResult> GetReceptTestComparisonResultsAsync(TestPriceComparisonFilterQuery query)
    {
        var items = receptionRepository.GetReceptTestComparisonResults(query);
        return Task.FromResult<OperationResult>(OperationResult<List<ReceptTestComparisonResult>>.Success(items));
    }

    public async Task<OperationResult> GetReceptionsByFilterAsync(ReceiveReceptionGroupFilterQuery filter)
    {
        var resolvedLabCode = await ResolveLegacyLabCodeAsync(filter.LabCode);
        if (resolvedLabCode == null)
            return OperationResult.Failure($"آزمایشگاه با کد {filter.LabCode} یافت نشد");

        filter.LabCode = resolvedLabCode.Value;

        if (filter.SourceLabCodes is { Count: > 0 })
        {
            var resolvedSources = new List<int>();
            foreach (var code in filter.SourceLabCodes.Distinct())
            {
                var resolved = await ResolveLegacyLabCodeAsync(code);
                if (resolved == null)
                    return OperationResult.Failure($"آزمایشگاه با کد {code} یافت نشد");
                resolvedSources.Add(resolved.Value);
            }

            filter.SourceLabCodes = resolvedSources.Distinct().ToList();
        }

        var items = receptionRepository.GetReceptionsByFilter(filter);
        return OperationResult<IReadOnlyList<ReceptionViewModel>>.Success(items);
    }

    /// <summary>
    /// Maps 5-digit lab codes (intLabIdNew) to legacy 4-digit intLabId used in reception tables.
    /// </summary>
    private async Task<int?> ResolveLegacyLabCodeAsync(int labCode)
    {
        if (labCode <= 0)
            return null;

        // Already legacy 4-digit (or rare 6-digit legacy id).
        if ((labCode is >= 1000 and <= 9999) || (labCode is >= 100000 and <= 999999))
            return labCode;

        var lab = await srLabRepository.GetSRLabName(labCode);
        return lab is { intLabId: > 0 } ? lab.intLabId : null;
    }

    public async Task<OperationResult> GetReportingByReceptIdAsync(GetReceptionQuery query)
    {
        var reception = await receptionRepository.GetReceptionByReceptId(query.sourceLabId, query.chrSourceReceptId);
        if (reception != null)
            return OperationResult<ReceptionViewModel>.Success(ReceptionModelConverter.ConvertToReceptionViewModel(reception));
        return OperationResult<ReceptionViewModel>.Failure("پذیرشی یافت نگردید");
    }

    public async Task<OperationResult> UpdateTargetReceptIdAsync(List<UpdateTargetReceptIdCommand> command)
    {
        receptionRepository.UpdateTargetReceptId(command);
        return await receptionRepository.SaveChangesAsync();
    }

    public Task<OperationResult> ClearReceiverReceptionsAsync(ClearReceiverReceptionCommand command)
    {
        if (command?.Items == null || command.Items.Count == 0)
            return Task.FromResult(OperationResult.Failure("موردی برای حذف پذیرش در دریافت‌کننده ارسال نشده است"));

        var cleared = receptionRepository.ClearReceiverReceptions(command.Items);
        if (cleared == 0)
            return Task.FromResult(OperationResult.Failure("آزمایشی با نتیجه خالی برای پاک‌سازی یافت نشد"));

        return Task.FromResult(OperationResult.SuccessResult());
    }

    public async Task<OperationResult> UpdateResultAsync(List<UpdateReportingItemsCommand> reportingItems)
    {
        var result = receptionRepository.UpdateResult(reportingItems);
        await receptionRepository.SaveChangesAsync();
        return OperationResult<IReadOnlyList<AddReceptTestNewResponse>>.Success(result!);
    }

    public Task<OperationResult> GetReportingItemsByFilterAsync(ReceiveGroupReportingItemFilter filter)
    {
        var receptions = receptionRepository.GetReportingItemsByFilter(filter);
        return Task.FromResult<OperationResult>(OperationResult<IReadOnlyList<ReceptionViewModel>>.Success(receptions));
    }

    public async Task<OperationResult> UpdateResultReceiveDateAsync(List<UpdateReportingItemsCommand> reportingItems)
    {
        receptionRepository.UpdateResultReceiveDate(reportingItems);
        return await receptionRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> UpdateTrackingAsync(ChangeTrackingCommand model)
    {
        if (model.Ids.Count == 0) return OperationResult.Failure("model Is Null");
        foreach (var id in model.Ids)
            receptionRepository.UpdateTracking(id.Value, model.Tracking);
        return await receptionRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> GetLabDetailAsync(int labcode)
    {
        var res = await srLabRepository.GetSRLabName(labcode);
        if (res == null)
            return OperationResult<SRLabNameViewModel>.Failure("آزمایشگاه یافت نشد");
        return OperationResult<SRLabNameViewModel>.Success(res);
    }

    public async Task<OperationResult> GetLabsAsync(List<int> labcodes)
    {
        var res = await srLabRepository.GetSRLabNameList(labcodes);
        return OperationResult<IReadOnlyList<SRLabNameViewModel>>.Success(res);
    }

    public async Task<OperationResult> GetSourcesLabNameAsync(int labcode)
    {
        var res = await srLabRepository.GetSourceSenderLabName(labcode);
        return OperationResult<IReadOnlyList<SRLabNameViewModel>>.Success(res);
    }

    public async Task<OperationResult> GetTargetsLabNameAsync(int labcode)
    {
        var res = await srLabRepository.GetTargetLabName(labcode);
        return OperationResult<IReadOnlyList<SRLabNameViewModel>>.Success(res);
    }

    public async Task<OperationResult> RemoveReceptTestAsync(long receptTestId)
    {
        var receptTestNew = receptionRepository.GetReceptTestById(receptTestId);
        if (receptTestNew != null)
        {
            if (string.IsNullOrEmpty(receptTestNew.chrTargetReceptId))
            {
                receptionRepository.DeleteReceptTest(receptTestNew);
                await receptionRepository.SaveChangesAsync();
            }
            var reception = await receptionRepository.GetReceptionByReceptId(receptTestNew.intSourceLabId, receptTestNew.chrSourceReceptId);
            if (reception != null && !reception.ReceptTests!.Any() && string.IsNullOrEmpty(reception.chrTargetReceptId))
            {
                receptionRepository.Delete(reception);
                await receptionRepository.SaveChangesAsync();
            }
        }
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> RejectReceptTestsAsync(RejectReceptTestCommand model)
    {
        if (model.Items == null || model.Items.Count == 0)
            return OperationResult.SuccessResult();

        foreach (var item in model.Items)
            receptionRepository.RejectTest(item);
        return await receptionRepository.SaveChangesAsync();
    }

    public Task<OperationResult> GetRejectCountAsync(int labCode)
    {
        var list = receptionRepository.GetRejectCount(labCode);
        return Task.FromResult<OperationResult>(OperationResult<List<string>?>.Success(list));
    }

    public async Task<OperationResult> GetAllSRLabs(int labCode, bool incoming)
    {
        var res = await srLabRepository.GetAllSRLabs(labCode, incoming);
        return OperationResult<IReadOnlyList<SRLabNameViewModel>>.Success(res);
    }

    public async Task<OperationResult<AdminReceptionDashboardStatsDto>> GetAdminReceptionDashboardStatsAsync(
        Guid userId,
        int? labCode = null,
        AdminReceptionDashboardSection? section = null)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult<AdminReceptionDashboardStatsDto>.Failure("کاربر یافت نشد");

        if (labCode.HasValue)
        {
            if (user.UserType is not (UserType.AdminLab or UserType.UserLab))
                return OperationResult<AdminReceptionDashboardStatsDto>.Failure("دسترسی مجاز نیست");

            if (!user.GetLabCode().HasValue || user.GetLabCode() != labCode)
                return OperationResult<AdminReceptionDashboardStatsDto>.Failure("دسترسی مجاز نیست");
        }
        else if (user.UserType != UserType.Administrator)
        {
            return OperationResult<AdminReceptionDashboardStatsDto>.Failure("دسترسی مجاز نیست");
        }

        var stats = await receptionRepository.GetAdminReceptionDashboardStatsAsync(labCode, section);
        await ApplyLabNamesAsync(stats);
        return OperationResult<AdminReceptionDashboardStatsDto>.Success(stats);
    }

    private async Task ApplyLabNamesAsync(AdminReceptionDashboardStatsDto stats)
    {
        var labCodes = new HashSet<int>();
        CollectLabCode(labCodes, stats.DailyMostUsage.LabCode);
        CollectLabCode(labCodes, stats.MonthlyMostUsage.LabCode);
        CollectLabCode(labCodes, stats.DailyMostSentTests.LabCode);
        CollectLabCode(labCodes, stats.MonthlyMostSentTests.LabCode);
        CollectLabCode(labCodes, stats.DailyMostReceivedTests.LabCode);
        CollectLabCode(labCodes, stats.MonthlyMostReceivedTests.LabCode);
        foreach (var row in stats.DailyLabs) labCodes.Add(row.LabCode);
        foreach (var row in stats.MonthlyLabs) labCodes.Add(row.LabCode);
        foreach (var row in stats.DailySentLabs) labCodes.Add(row.LabCode);
        foreach (var row in stats.MonthlySentLabs) labCodes.Add(row.LabCode);
        foreach (var row in stats.DailyReceivedLabs) labCodes.Add(row.LabCode);
        foreach (var row in stats.MonthlyReceivedLabs) labCodes.Add(row.LabCode);

        var nameMap = await GetLabNameMapAsync(labCodes);
        ApplyLabName(stats.DailyMostUsage, nameMap);
        ApplyLabName(stats.MonthlyMostUsage, nameMap);
        ApplyLabName(stats.DailyMostSentTests, nameMap);
        ApplyLabName(stats.MonthlyMostSentTests, nameMap);
        ApplyLabName(stats.DailyMostReceivedTests, nameMap);
        ApplyLabName(stats.MonthlyMostReceivedTests, nameMap);
        ApplyLabNames(stats.DailyLabs, nameMap);
        ApplyLabNames(stats.MonthlyLabs, nameMap);
        ApplyLabNames(stats.DailySentLabs, nameMap);
        ApplyLabNames(stats.MonthlySentLabs, nameMap);
        ApplyLabNames(stats.DailyReceivedLabs, nameMap);
        ApplyLabNames(stats.MonthlyReceivedLabs, nameMap);
    }

    private async Task<Dictionary<int, string>> GetLabNameMapAsync(IEnumerable<int> labCodes)
    {
        var codes = labCodes.Distinct().ToList();
        if (codes.Count == 0)
            return new Dictionary<int, string>();

        var labs = await srLabRepository.GetSRLabNameList(codes);
        var map = new Dictionary<int, string>();
        foreach (var lab in labs)
        {
            var name = lab.vchLabName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
                continue;

            map[lab.intLabId] = name;
            if (lab.intLabIdNew.HasValue)
                map[lab.intLabIdNew.Value] = name;
        }

        return map;
    }

    private static void CollectLabCode(ISet<int> labCodes, int? labCode)
    {
        if (labCode.HasValue)
            labCodes.Add(labCode.Value);
    }

    private static void ApplyLabName(TopLabMetricDto metric, IReadOnlyDictionary<int, string> nameMap)
    {
        if (metric.LabCode.HasValue && nameMap.TryGetValue(metric.LabCode.Value, out var labName))
            metric.LabName = labName;
    }

    private static void ApplyLabNames(List<LabReceptionSummaryRowDto> rows, IReadOnlyDictionary<int, string> nameMap)
    {
        foreach (var row in rows)
        {
            if (nameMap.TryGetValue(row.LabCode, out var labName))
                row.LabName = labName;
        }
    }
}
