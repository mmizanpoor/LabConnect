using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.PaymentGateway;
using LaboratoryApi.Models.DTO.PaymentGateway;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations
{
    public class BehPardakhtService : IBehPardakhtService
    {
        private long teminalId = 9015104;
        private string username = "tebonarmafzar021";
        private string password = "84321046";
        public CallbackResponseStatus CheckCallbackResponse(CallbackResponse callbackResponse)
        {
            return new CallbackResponseStatus() { IsSuccess = true };
        }

        public async Task<InitialRequestResponseDto> InitialRequest(InitialRequestDto initialRequest)
        {
            var initialRequestResponse = new InitialRequestResponseDto();

            var bpService = new BehPardakhtProd.PaymentGatewayClient();
            var request = await bpService.bpPayRequestAsync(teminalId, username, password, long.Parse(initialRequest.SystemTransactionKey), (int)initialRequest.Amount, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), "", GenerateCallBackUrl(initialRequest.Domain), "0", "", "", "", "", "");

            var body = Task.FromResult(request).Result.Body;
            var result = body.@return;

            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains(','))
                {
                    var resultArray = result.Split(",");

                    if (resultArray[0] == "0")
                    {
                        initialRequestResponse = new InitialRequestResponseDto()
                        {
                            RedirectUrl = $"{Constants.Constants.BEHPARDAKHT_URL}/pgwchannel/startpay.mellat",
                            Key = resultArray[1].ToString(),
                            IsSuccess = true,
                            PaymentGatewayType = PaymentGatewayType.BehPardakht
                        };
                    }
                }
                else
                {
                    string message = ShowErrorBehPrdakht(result, "");

                    initialRequestResponse = new InitialRequestResponseDto()
                    {
                        RedirectUrl = null,
                        Key = "",
                        Message = message,
                        IsSuccess = false,
                        PaymentGatewayType = PaymentGatewayType.BehPardakht
                    };

                }
            }

            return initialRequestResponse;
        }

        public async Task<VerifyResponseDto> VerifyPay(VerifyRequestDto verifyRequestDto)
        {
            if (verifyRequestDto.Code == "")
            {
                return new VerifyResponseDto() { Message = "اطلاعات پرداخت یافت نگردید", IsSuccess = false };
            }

            if (verifyRequestDto.Code != "0")
                return new VerifyResponseDto() { Message = ShowErrorBehPrdakht(verifyRequestDto.Code, "CallBack"), IsSuccess = false };

            try
            {
                var bpService = new BehPardakhtProd.PaymentGatewayClient();
                var verfyResponse = await bpService.bpVerifyRequestAsync(teminalId,
                    username,
                    password,
                    long.Parse(verifyRequestDto.SystemTransactionKey),
                    long.Parse(verifyRequestDto.SystemTransactionKey),
                    long.Parse(verifyRequestDto.Key));

                var verifyBody = Task.FromResult(verfyResponse).Result.Body;
                var result = verifyBody.@return;

                if (!string.IsNullOrWhiteSpace(result) && result == "0" || result == "موفق")
                {
                    return new VerifyResponseDto() { IsSuccess = true, Message = $"پرداخت شما با موفقیت انجام شد - {result}", RefId = verifyRequestDto.Key, Code = result };
                }
                else if(int.TryParse(result, out _))
                    return new VerifyResponseDto() { Message = $"{ShowErrorBehPrdakht(result, "")} - {result}", IsSuccess = false };
                else return new VerifyResponseDto() { Message = $"عدم دریافت پاسخ تایید پرداخت از سمت بانک. در صورت کسر وجه، مبلغ به حساب شما برگشت داده خواهد شد. - {result}", IsSuccess = false };
            }
            catch (Exception ex)
            {
                return new VerifyResponseDto() { Message = "دریافت خطای نامعلوم از سمت پذیرنده. در صورت کسر وجه، مبلغ به حساب شما برگشت داده خواهد شد", IsSuccess = false };
            }
        }
        private string GenerateCallBackUrl(string domain)
        {
            return $"{domain}/CenterProfile/VerifyPayMellat";
        }

        private string ShowErrorBehPrdakht(string resCode, string pageType)
        {
            string error = "";

            if (pageType == "CallBack")
            {
                error += "پرداخت بدلیل زیر موفقیت آمیز نبوده است و در صورت کسر وجه به حساب شما برخواهد گشت.";
                error += "<br/>";
            }

            switch (resCode)
            {
                case "11":
                    error += "اطلاعات پرداخت یافت نشد";
                    break;
                case "12":
                    error += "موجودی کافي نيست";
                    break;
                case "13":
                    error += "رمز نادرست است";
                    break;
                case "14":
                    error += "تعداد دفعات وارد کردن رمز بيش از حد مجاز است";
                    break;
                case "15":
                    error += "کارت نامعتبر است";
                    break;
                case "16":
                    error += "دفعات برداشت وجه بيش از حد مجاز است";
                    break;
                case "17":
                    error += "کاربر از انجام تراکنش منصرف شده است";
                    break;
                case "18":
                    error += "تاريخ انقضای کارت گذشته است";
                    break;
                case "19":
                    error += "مبلغ برداشت وجه بيش از حد مجاز است";
                    break;
                case "111":
                    error += "صادر کننده کارت نامعتبر است";
                    break;
                case "113":
                    error += "پاسخي از صادر کننده کارت دريافت نشد";
                    break;
                case "114":
                    error += "دارنده کارت مجاز به انجام اين تراکنش نيست";
                    break;
                case "21":
                    error += "پذيرنده نامعتبر است";
                    break;
                case "24":
                    error += "اطلاعات کاربری پذيرنده نامعتبر است";
                    break;
                case "25":
                    error += "مبلغ نامعتبر است";
                    break;
                case "33":
                    error += "حساب نامعتبر است";
                    break;
                case "35":
                    error += "تاريخ نامعتبر است";
                    break;
                case "48":
                    error += "تراکنش عودت شده است";
                    break;
                case "413":
                    error += "شناسه پرداخت نادرست است";
                    break;
                case "416":
                    error += "خطا در ثبت اطلاعات";
                    break;
                case "421":
                    error += "آدرس نامعتبر است";
                    break;
                case "51":
                    error += "تراکنش تکراری است";
                    break;
                case "54":
                    error += "تراکنش مرجع موجود نيست";
                    break;
                case "55":
                    error += "تراکنش نامعتبر است";
                    break;
                case "61":
                    error += "خطا در واریز";
                    break;
                case "62":
                    error += "مسیر بازگشتی از درگاه پرداخت نادرست است";
                    break;
                default:
                    error += "خطا نامشخص";
                    break;
            }

            return error;
        }
    }
}
