using System.Text;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Rasa;
using Newtonsoft.Json;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class RasaService(HttpClient httpClient) : IRasaService
{
    private const string BaseUrl = "https://epi.rasatpa.ir";
    //private const string BaseUrl = "https://dina.demisco.com:30117";
    private const string TokenPath = "/auth/oauth/token";
    private const string BasicAuthorization = "Basic ZGluYUFwcElkOnRlc3Q=";
    private const string Cookie = "dincok=dina4";
    private const string ApiCookie = "dincok=dina2";
    private const string UsernameTest = "azmoon_lab";
    private const string Username = "azmoonlab";
    private const string Password = "@Zmoon792025";
    private const string GrantType = "password";
    private const string ContractListPath = "/hcp-ws/api/v1/dina/getContractList";
    private const string SaveDinaPath = "/hcp-ws/api/v1/dina/save-dina";
    private const string CancelDinaPath = "/hcp-ws/api/v1/dina/cancel-dina";

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Include,
        DefaultValueHandling = DefaultValueHandling.Include,
    };

    private static readonly JsonSerializerSettings DinaApiJsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
    };

    private string? _accessToken;

    public async Task<OperationResult> GetOAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(Username), "username");
        form.Add(new StringContent(Password), "password");
        form.Add(new StringContent(GrantType), "grant_type");

        var result = await SendAsync<RasaOAuthTokenDto>(
            TokenPath,
            form,
            new Dictionary<string, string>
            {
                ["Authorization"] = BasicAuthorization,
                ["Cookie"] = Cookie,
            },
            cancellationToken);

        if (!result.Status || result.Data == null)
            return OperationResult.Failure(result.Message ?? "دریافت توکن Rasa ناموفق بود");

        _accessToken = result.Data.AccessToken;
        return OperationResult<RasaOAuthTokenDto>.Success(result.Data);
    }

    public async Task<OperationResult> GetContractListAsync(
        RasaContractListCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("توکن دسترسی موجود نیست");

        var json = JsonConvert.SerializeObject(command, DinaApiJsonSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = await SendAsync<RasaGetContractListResponseDto>(
            ContractListPath,
            content,
            new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Cookie"] = ApiCookie,
                ["Content-Type"] = "application/json",
            },
            cancellationToken);

        if (!result.Status || result.Data == null)
        {
            var rasaResponse = JsonConvert.DeserializeObject<ResponseRasa>(result.Message!);
            if (rasaResponse?.Messages?.Any() == true)
            {
                var messages = string.Join(
                    Environment.NewLine,
                    rasaResponse.Messages.Select(x => $"{x.Status}:{x.Description}"));

                return OperationResult.Failure(messages);
            }
            return OperationResult.Failure(result.Message ?? "دریافت لیست معرفی‌نامه ناموفق بود");
        }

        if (result.Data != null && result.Data.ErrorDetail != null)
            return OperationResult.Failure(result.Data.ErrorDetail.ErrorMessage ?? "دریافت لیست معرفی‌نامه ناموفق بود");

        return ToDinaOperationResult(result.Data, "دریافت لیست معرفی‌نامه ناموفق بود");
    }

    public async Task<OperationResult> ValidationDinaAsync(
        RasaClaimInfoModelCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("توکن دسترسی موجود نیست");

        var json = JsonConvert.SerializeObject(command, JsonSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = await SendAsync<RasaValidationDinaResponseDto>(
            SaveDinaPath,
            content,
            new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Cookie"] = ApiCookie,
            },
            cancellationToken);

        if (!result.Status || result.Data == null)
            return OperationResult.Failure(result.Message ?? "اعتبارسنجی Dina ناموفق بود");

        return OperationResult<RasaValidationDinaResponseDto>.Success(result.Data);
    }

    public async Task<OperationResult> CancelDinaAsync(
        RasaCancelDinaCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = await EnsureAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("توکن دسترسی موجود نیست");

        var json = JsonConvert.SerializeObject(command, JsonSettings);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var result = await SendAsync<RasaCancelDinaResponseDto>(
            CancelDinaPath,
            content,
            new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Cookie"] = ApiCookie,
            },
            cancellationToken);

        if (!result.Status || result.Data == null)
            return OperationResult.Failure(result.Message ?? "حذف Dina ناموفق بود");

        return ToDinaOperationResult(result.Data, "حذف Dina ناموفق بود");
    }

    private static OperationResult<T> ToDinaOperationResult<T>(T data, string fallbackMessage)
        where T : class, IRasaDinaResponse
    {
        if (!string.IsNullOrWhiteSpace(data.ErrorDetail?.ErrorMessage))
        {
            return new OperationResult<T>
            {
                Status = false,
                Message = data.ErrorDetail.ErrorMessage.Trim(),
                Data = data,
            };
        }

        return OperationResult<T>.Success(data);
    }

    private async Task<string?> EnsureAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken))
            return _accessToken;

        var tokenResult = await GetOAuthTokenAsync(cancellationToken);
        if (tokenResult is OperationResult<RasaOAuthTokenDto> typed && typed.Status)
            return typed.Data?.AccessToken;

        return null;
    }

    //public Task<OperationResult<TResponse>> PostAsync<TResponse>(
    //    string endpoint,
    //    object data,
    //    CancellationToken cancellationToken = default)
    //{
    //    var json = JsonConvert.SerializeObject(data, JsonSettings);

    //    var content = new StringContent(json, Encoding.UTF8, "application/json");
    //    var path = $"/Api/Rasa/{endpoint.TrimStart('/')}";

    //    return SendAsync<TResponse>(path, content, cancellationToken: cancellationToken);
    //}

    private async Task<OperationResult<T>> SendAsync<T>(
        string path,
        HttpContent content,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = content;

            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    if (key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        if (request.Content != null)
                            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(value);
                        continue;
                    }

                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return OperationResult<T>.Failure(string.IsNullOrWhiteSpace(body) ? $"خطای HTTP {(int)response.StatusCode}" : body);

            var data = JsonConvert.DeserializeObject<T>(body);
            if (data == null)
                return OperationResult<T>.Failure("پاسخ سرویس Rasa نامعتبر است");

            return OperationResult<T>.Success(data);
        }
        catch (Exception ex)
        {
            return OperationResult<T>.Failure(ex.Message);
        }
    }
}
