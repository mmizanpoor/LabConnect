using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SpecialOffer;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISpecialOfferService
{
    Task<OperationResult<List<SpecialOfferListItemDto>>> GetAllAsync(Guid userId);
    Task<OperationResult<SpecialOfferDetailDto>> GetByIdAsync(Guid userId, long id);
    Task<OperationResult<SpecialOfferDetailDto>> CreateAsync(Guid userId, CreateSpecialOfferCommand command);
    Task<OperationResult<SpecialOfferDetailDto>> UpdateAsync(Guid userId, UpdateSpecialOfferCommand command);
    Task<OperationResult> DeleteAsync(Guid userId, long id);
    Task<OperationResult<List<SpecialOfferRequestDto>>> GetRequestsAsync(Guid userId, long specialOfferId);
    Task<OperationResult> ApproveRequestAsync(Guid userId, long requestId);
    Task<OperationResult> RejectRequestAsync(Guid userId, RejectSpecialOfferRequestCommand command);
    Task<OperationResult<SpecialOfferRequestForAgreementDto>> GetRequestForAgreementAsync(Guid userId, long requestId);
    Task<OperationResult<SpecialOfferDashboardStatsDto>> GetAdminDashboardStatsAsync();
    Task<OperationResult<List<AdminSpecialOfferListItemDto>>> GetAllForAdminAsync();
    Task<OperationResult<AdminSpecialOfferDetailDto>> GetByIdForAdminAsync(long id);
}

public interface IPublicSpecialOfferService
{
    Task<OperationResult<List<PublicSpecialOfferCardDto>>> GetActiveForHomeAsync();
    Task<OperationResult<PublicSpecialOfferDetailDto>> GetDetailAsync(long id, Guid? userId);
    Task<OperationResult> SubmitRequestAsync(long id, Guid userId, SubmitSpecialOfferRequestCommand command);
}

public interface ILabAgreementPortalService
{
    Task<OperationResult<long>> CreateFromPortalAsync(Guid userId, CreateLabAgreementFromPortalCommand command);
}
