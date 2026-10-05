using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.TestInfo;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ITestInfoService
{
    Task<OperationResult<List<TestInfoListItemDto>>> GetAllAsync(Guid userId);

    Task<OperationResult<TestInfoDetailDto>> GetByIdAsync(Guid userId, long id);

    Task<OperationResult<TestInfoDetailDto>> UpdateApprovePriceAsync(Guid userId, UpdateTestInfoApprovePriceCommand command);

    Task<OperationResult<TestInfoDetailDto>> UpdateTestInfoAsync(Guid userId, UpdateTestInfoCommand command);

    Task<OperationResult<SaveSendTestInfoResultsResponse>> SaveSendTestInfoResultsAsync(List<SendTestInfoResult> items);
}
