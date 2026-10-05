using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;
using LaboratoryApi.Models.DTO.PaymentGateway;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<InitialRequestResponseDto> InitialRequest(InitialRequestDto initialRequest);
        CallbackResponseStatus CheckCallbackResponse(CallbackResponse callbackResponse);
        Task<VerifyResponseDto> VerifyPay(VerifyRequestDto verifyRequestDto);
    }
}
