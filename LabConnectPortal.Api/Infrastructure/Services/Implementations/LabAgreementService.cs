using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LabAgreementService(ILabAgreementRepository repository, IUserRepository userRepository) : ILabAgreementService
{
    public async Task<OperationResult> IsExistLabAgreement(GetExistLabAgreementQuery query)
    {
        var res = repository.IsExistLabAgreement(query);
        return OperationResult<bool>.Success(res!);
    }

    public async Task<OperationResult> CreateAsync(LabAgreementCommand command)
        => await repository.InsertLabAgreement(command);

    public async Task<OperationResult> RemoveLabAgreementAsync(long id)
    {
        var labAgreement = await repository.GetByIdAsync(id) as Domain.Entities.LabAgreement;
        if (labAgreement != null && string.IsNullOrEmpty(labAgreement.ReceiverAgreementSign))
        {
            await repository.DeleteLabAgreementTreeAsync(id);
            return await repository.SaveChangesAsync();
        }
        return OperationResult.SuccessResult();
    }

    public async Task<OperationResult> GetLabAgreementByIdAsync(int id)
    {
        var res = await repository.GetLabAgreementById(id);
        if (res != null)
            res.MergedTestPrices = await repository.GetMergedTestPricesAsync(id);

        return OperationResult<LabAgreementCommand>.Success(res!);
    }

    public async Task<OperationResult> GetLabAgreementAsync(long id)
    {
        var res = await repository.GetLabAgreement(id);
        if (res == null)
            return OperationResult.Failure("قرارداد یافت نشد");

        return OperationResult<LabAgreementCommand>.Success(res);
    }

    public async Task<OperationResult> GetLabAgreementByLabCodeAsync(GetLabAgreementByLabCodeQuery query)
    {
        var res = await repository.GetLabAgreementByLabCode(query.PrimaryLabCode, query.ReceiverLabCode);
        return OperationResult<LabAgreementCommand?>.Success(res);
    }

    public Task<OperationResult> GetLabAgreementsAsync(GetLabAgreementQuery query)
    {
        var items = repository.GetLabAgreements(query);
        return Task.FromResult<OperationResult>(OperationResult<IReadOnlyList<LabAgreementCommand>>.Success(items));
    }

    public Task<OperationResult> GetAllLabAgreementsAsync(GetAllLabAgreementQuery query)
    {
        var items = repository.GetAllLabAgreements(query);
        return Task.FromResult<OperationResult>(OperationResult<IReadOnlyList<LabAgreementCommand>>.Success(items));
    }

    public async Task<OperationResult> ReceiverSignAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverSign(command));

    public async Task<OperationResult> ReceiverSeenAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverActionSeen(command));

    public async Task<OperationResult> ReceiverRejectAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverReject(command));

    public async Task<OperationResult> PrimarySignAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.PrimarySign(command));

    public async Task<OperationResult> AddTestPricesAsync(LabAgreementCommand command)
    {
        repository.AddTestPrices(command);
        return await repository.SaveChangesAsync();
    }

    public async Task<OperationResult> PrimarySuspendAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.PrimarySuspendAgreement(command));

    public async Task<OperationResult> PrimaryTerminationAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.PrimaryTerminationAgreement(command));

    public async Task<OperationResult> ReceiverSuspendAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverSuspendAgreement(command));

    public async Task<OperationResult> ReceiverTerminationAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverTerminationAgreement(command));

    public async Task<OperationResult> ReceiverCanceledSuspendAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverCanceledSuspendAgreement(command));

    public async Task<OperationResult> PrimaryCanceledSuspendAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.PrimaryCanceledSuspendAgreement(command));

    public async Task<OperationResult> PrimaryCanceledTerminationAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.PrimaryCanceledTerminationAgreement(command));

    public async Task<OperationResult> ReceiverCanceledTerminationAgreementAsync(LabAgreementCommand command)
        => await SignActionAsync(command, () => repository.ReceiverCanceledTerminationAgreement(command));

    public async Task<OperationResult> UpdateLabAgreementAsync(LabAgreementCommand command)
    {
        if (!command.Id.HasValue)
            return OperationResult.Failure("شناسه قرارداد الزامی است");

        if (!repository.UpdateLabAgreement(command))
            return OperationResult.Failure("قرارداد یافت نشد");

        var result = await repository.SaveChangesAsync();
        if (!result.Success)
            return result;

        var updated = await repository.GetLabAgreementById(command.Id.Value);
        return OperationResult<LabAgreementCommand?>.Success(updated);
    }

    public async Task<OperationResult> UpdateLaboratoryAgreementStateAsync(UpdateLaboratoryAgreementStateCommand command)
    {
        if (!repository.UpdateLaboratoryAgreementState(command.Id, command.LaboratoryAgreementState))
            return OperationResult.Failure("قرارداد یافت نشد");

        return await repository.SaveChangesAsync();
    }

    public async Task<OperationResult<LabAgreementStatsDto>> GetGlobalStatsAsync()
    {
        var stats = await repository.GetGlobalStatsAsync();
        return OperationResult<LabAgreementStatsDto>.Success(stats);
    }

    public async Task<OperationResult<LabAgreementLabStatsDto>> GetStatsForCurrentLabAsync(Guid userId)
    {
        var user = await userRepository.GetWithRolesAsync(userId);
        if (user == null)
            return OperationResult<LabAgreementLabStatsDto>.Failure("کاربر یافت نشد");

        if (user.UserType is not (UserType.AdminLab or UserType.UserLab))
            return OperationResult<LabAgreementLabStatsDto>.Failure("دسترسی مجاز نیست");

        var labCodeNew = user.GetLabCodeNew() ?? user.GetLabCode();
        if (!labCodeNew.HasValue)
            return OperationResult<LabAgreementLabStatsDto>.Failure("کد آزمایشگاه تعریف نشده است");

        var stats = await repository.GetStatsByLabCodeAsync(labCodeNew.Value);
        return OperationResult<LabAgreementLabStatsDto>.Success(stats);
    }

    public async Task<OperationResult<ActiveContractLaboratoryResult>> GetActiveContractLaboratory(GetActiveContractLaboratoryQuery query)
    {
        var items = await repository.GetActiveContractLaboratory(query);
        return OperationResult<ActiveContractLaboratoryResult>.Success(items);
    }

    private async Task<OperationResult> SignActionAsync(LabAgreementCommand command, Action action)
    {
        action();
        var result = await repository.SaveChangesAsync();
        if (!result.Success)
            return result;

        var labAgreement = await repository.GetLabAgreementById(command.Id!.Value);
        return OperationResult<LabAgreementCommand?>.Success(labAgreement);
    }
}
