using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.TestInfo;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class TestInfoService(LabConnectDbContext context, IUserRepository userRepository) : ITestInfoService
{
    public async Task<OperationResult<List<TestInfoListItemDto>>> GetAllAsync(Guid userId)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<List<TestInfoListItemDto>>.Failure(access.Error);

        var items = await context.TestInfos
            .AsNoTracking()
            .Where(t => t.LabCodeNew == access.LabCodeNew)
            .OrderBy(t => t.FullName ?? t.MeasurName)
            .Select(t => new TestInfoListItemDto
            {
                Id = t.Id,
                CPNCode = t.CPNCode,
                NationalCode = t.NationalCode,
                FullName = t.FullName,
                ShortName = t.ShortName,
                SectionName = t.SectionName,
                ApprovePrice = t.ApprovePrice,
            })
            .ToListAsync();

        return OperationResult<List<TestInfoListItemDto>>.Success(items);
    }

    public async Task<OperationResult<TestInfoDetailDto>> GetByIdAsync(Guid userId, long id)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<TestInfoDetailDto>.Failure(access.Error);

        var item = await context.TestInfos
            .AsNoTracking()
            .Include(t => t.KitGroup)
            .Include(t => t.DeviceGroup)
            .FirstOrDefaultAsync(t => t.Id == id && t.LabCodeNew == access.LabCodeNew);

        if (item == null)
            return OperationResult<TestInfoDetailDto>.Failure("آزمایش یافت نشد");

        return OperationResult<TestInfoDetailDto>.Success(MapToDetailDto(item));
    }

    public async Task<OperationResult<TestInfoDetailDto>> UpdateApprovePriceAsync(
        Guid userId,
        UpdateTestInfoApprovePriceCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<TestInfoDetailDto>.Failure(access.Error);

        var item = await context.TestInfos
            .Include(t => t.KitGroup)
            .Include(t => t.DeviceGroup)
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.LabCodeNew == access.LabCodeNew);

        if (item == null)
            return OperationResult<TestInfoDetailDto>.Failure("آزمایش یافت نشد");

        if (command.ApprovePrice.HasValue && command.ApprovePrice.Value < 0)
            return OperationResult<TestInfoDetailDto>.Failure("قیمت مبنا نامعتبر است");

        item.ApprovePrice = command.ApprovePrice;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult<TestInfoDetailDto>.Failure("خطا در ذخیره‌سازی");
        }

        return OperationResult<TestInfoDetailDto>.Success(MapToDetailDto(item));
    }

    public async Task<OperationResult<TestInfoDetailDto>> UpdateTestInfoAsync(
        Guid userId,
        UpdateTestInfoCommand command)
    {
        var access = await GetLabCodeNewAsync(userId);
        if (access.Error != null)
            return OperationResult<TestInfoDetailDto>.Failure(access.Error);

        var item = await context.TestInfos
            .Include(t => t.KitGroup)
            .Include(t => t.DeviceGroup)
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.LabCodeNew == access.LabCodeNew);

        if (item == null)
            return OperationResult<TestInfoDetailDto>.Failure("آزمایش یافت نشد");

        if (command.ApprovePrice.HasValue && command.ApprovePrice.Value < 0)
            return OperationResult<TestInfoDetailDto>.Failure("قیمت مبنا نامعتبر است");

        var groupValidation = await ValidateGroupIdsAsync(
            access.LabCodeNew!.Value,
            command.KitGroupId,
            command.DeviceGroupId);
        if (groupValidation != null)
            return OperationResult<TestInfoDetailDto>.Failure(groupValidation);

        item.ApprovePrice = command.ApprovePrice;
        item.KitGroupId = command.KitGroupId;
        item.DeviceGroupId = command.DeviceGroupId;

        try
        {
            await context.SaveChangesAsync();
            await context.Entry(item).Reference(t => t.KitGroup).LoadAsync();
            await context.Entry(item).Reference(t => t.DeviceGroup).LoadAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult<TestInfoDetailDto>.Failure("خطا در ذخیره‌سازی");
        }

        return OperationResult<TestInfoDetailDto>.Success(MapToDetailDto(item));
    }

    private async Task<string?> ValidateGroupIdsAsync(int labCodeNew, long? kitGroupId, long? deviceGroupId)
    {
        if (kitGroupId.HasValue &&
            !await context.KitGroups.AnyAsync(g => g.Id == kitGroupId && g.LabCodeNew == labCodeNew))
            return "گروه کیت نامعتبر است";

        if (deviceGroupId.HasValue &&
            !await context.DeviceGroups.AnyAsync(g => g.Id == deviceGroupId && g.LabCodeNew == labCodeNew))
            return "گروه دستگاه نامعتبر است";

        return null;
    }

    public async Task<OperationResult<SaveSendTestInfoResultsResponse>> SaveSendTestInfoResultsAsync(
        List<SendTestInfoResult> items)
    {
        if (items == null || items.Count == 0)
            return OperationResult<SaveSendTestInfoResultsResponse>.Failure("لیست خالی است");

        var validItems = items
            .Where(i => i.LabCodeNew > 0 && i.TestId > 0)
            .GroupBy(i => (i.LabCodeNew, i.TestId))
            .Select(g => g.Last())
            .ToList();

        if (validItems.Count == 0)
            return OperationResult<SaveSendTestInfoResultsResponse>.Failure("هیچ رکورد معتبری یافت نشد");

        var insertedCount = 0;
        var updatedCount = 0;

        foreach (var labGroup in validItems.GroupBy(i => i.LabCodeNew))
        {
            var labCodeNew = labGroup.Key;
            var testIds = labGroup.Select(i => i.TestId).Distinct().ToList();

            var existingItems = await context.TestInfos
                .Where(t => t.LabCodeNew == labCodeNew && testIds.Contains(t.TestId))
                .ToListAsync();

            var existingByTestId = existingItems.ToDictionary(t => t.TestId);

            foreach (var item in labGroup)
            {
                if (existingByTestId.TryGetValue(item.TestId, out var existing))
                {
                    ApplySendTestInfoResult(existing, item, preserveApprovePrice: true);
                    updatedCount++;
                    continue;
                }

                var entity = MapToEntity(item);
                context.TestInfos.Add(entity);
                existingByTestId[item.TestId] = entity;
                insertedCount++;
            }
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return OperationResult<SaveSendTestInfoResultsResponse>.Failure("خطا در ذخیره‌سازی");
        }

        return OperationResult<SaveSendTestInfoResultsResponse>.Success(new SaveSendTestInfoResultsResponse
        {
            InsertedCount = insertedCount,
            UpdatedCount = updatedCount,
        });
    }

    private static TestInfo MapToEntity(SendTestInfoResult source)
    {
        var entity = new TestInfo();
        ApplySendTestInfoResult(entity, source, preserveApprovePrice: false);
        return entity;
    }

    private static void ApplySendTestInfoResult(
        TestInfo entity,
        SendTestInfoResult source,
        bool preserveApprovePrice)
    {
        entity.LabCode = source.LabCode;
        entity.LabCodeNew = source.LabCodeNew;
        entity.CPNCode = TrimToNull(source.CPNCode);
        entity.NationalCode = TrimToNull(source.NationalCode);
        entity.MeasurName = TrimToNull(source.MeasurName);
        entity.FullName = TrimToNull(source.FullName);
        entity.ShortName = TrimToNull(source.ShortName);
        entity.SectionName = TrimToNull(source.SectionName);
        entity.SimilarName = TrimToNull(source.SimilarName);
        entity.KD = TrimToNull(source.KD);
        entity.Volume = TrimToNull(source.Volume);
        entity.MinVolume = TrimToNull(source.MinVolume);
        entity.Maintenance = TrimToNull(source.Maintenance);
        entity.Transportation = TrimToNull(source.Transportation);
        entity.Needs = TrimToNull(source.Needs);
        entity.Guidance = TrimToNull(source.Guidance);
        entity.PatientInfo = TrimToNull(source.PatientInfo);
        entity.Denial = TrimToNull(source.Denial);
        entity.Preparation = TrimToNull(source.Preparation);
        entity.ClinicalInfo = TrimToNull(source.ClinicalInfo);
        entity.Sources = TrimToNull(source.Sources);
        entity.Comment = TrimToNull(source.Comment);
        entity.Caution = TrimToNull(source.Caution);
        entity.SClinical = TrimToNull(source.SClinical);
        entity.Detail = TrimToNull(source.Detail);
        entity.Date = TrimToNull(source.Date);
        entity.ResultDuration = TrimToNull(source.ResultDuration);
        entity.MaxDurResult = TrimToNull(source.MaxDurResult);
        entity.MaintenanceDur = TrimToNull(source.MaintenanceDur);
        entity.TestId = source.TestId;
        entity.Criteria = TrimToNull(source.Criteria);
        entity.Freezer = TrimToNull(source.Freezer);
        entity.DeliveryCondition = TrimToNull(source.DeliveryCondition);
        entity.KitGroupId = source.KitGroupId;
        entity.DeviceGroupId = source.DeviceGroupId;

        if (!preserveApprovePrice || source.ApprovePrice.HasValue)
            entity.ApprovePrice = source.ApprovePrice;
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static TestInfoDetailDto MapToDetailDto(Domain.Entities.TestInfo item) => new()
    {
        Id = item.Id,
        LabCode = item.LabCode,
        LabCodeNew = item.LabCodeNew,
        CPNCode = item.CPNCode,
        NationalCode = item.NationalCode,
        MeasurName = item.MeasurName,
        FullName = item.FullName,
        ShortName = item.ShortName,
        SectionName = item.SectionName,
        SimilarName = item.SimilarName,
        KD = item.KD,
        Volume = item.Volume,
        MinVolume = item.MinVolume,
        Maintenance = item.Maintenance,
        Transportation = item.Transportation,
        Needs = item.Needs,
        Guidance = item.Guidance,
        PatientInfo = item.PatientInfo,
        Denial = item.Denial,
        Preparation = item.Preparation,
        ClinicalInfo = item.ClinicalInfo,
        Sources = item.Sources,
        Comment = item.Comment,
        Caution = item.Caution,
        SClinical = item.SClinical,
        Detail = item.Detail,
        Date = item.Date,
        ResultDuration = item.ResultDuration,
        MaxDurResult = item.MaxDurResult,
        MaintenanceDur = item.MaintenanceDur,
        TestId = item.TestId,
        Criteria = item.Criteria,
        Freezer = item.Freezer,
        DeliveryCondition = item.DeliveryCondition,
        ApprovePrice = item.ApprovePrice,
        KitGroupId = item.KitGroupId,
        KitGroupTitle = item.KitGroup?.Title,
        DeviceGroupId = item.DeviceGroupId,
        DeviceGroupTitle = item.DeviceGroup?.Title,
    };

    private async Task<(int? LabCodeNew, string? Error)> GetLabCodeNewAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null || user.UserType is not (UserType.AdminLab or UserType.UserLab))
            return (null, "دسترسی مجاز نیست");

        if (!user.GetLabCodeNew().HasValue)
            return (null, "کد آزمایشگاه تعریف نشده است");

        return (user.GetLabCodeNew()!.Value, null);
    }
}
