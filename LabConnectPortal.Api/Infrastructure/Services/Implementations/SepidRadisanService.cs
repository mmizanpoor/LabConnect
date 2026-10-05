using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.SepidRadisan;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SepidRadisanService(
    HttpClient httpClient,
    ISepidRadisanRepository repository,
    IOptions<SepidRadisanSettings> settings) : ISepidRadisanService
{
    private const string InsurancersListPath = "api/Insurancers/List";
    private const string InquiryPath = "api/Insured/Inquiry";
    private const string PolicyInfoPath = "api/Policies/Info";
    private const string InsuredInfoPath = "api/Insured/Info";
    private const string PreCheckIntroductionPath = "api/Introductions/PreCheckIntroduction";
    private const string CreateIntroductionPath = "api/Introductions/Create";

    private static readonly JsonSerializerSettings PreCheckRequestJsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.None,
    };

    private readonly SepidRadisanSettings _settings = settings.Value;

    public async Task<OperationResult> GetInsurancersListAsync(
        SepidRadisanGetInsurancersCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendGetAsync<List<SepidRadisanInsurancerDto>>(
            InsurancersListPath,
            token,
            query: null,
            insurancer: null,
            cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendGetAsync<List<SepidRadisanInsurancerDto>>(
                InsurancersListPath,
                token,
                query: null,
                insurancer: null,
                cancellationToken);
        }

        if (!result.Success)
            return OperationResult.Failure(result.ErrorMessage ?? "دریافت لیست بیمه‌گرها ناموفق بود");

        return OperationResult<List<SepidRadisanInsurancerDto>>.Success(result.Data ?? []);
    }

    public async Task<OperationResult> GetInquiryPoliciesAsync(
        SepidRadisanInquiryCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.NationalCode))
            return OperationResult.Failure("کد ملی الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var inquiryResult = await SendGetAsync<List<SepidRadisanInquiryDto>>(
            InquiryPath,
            token,
            query: $"NationalCode={Uri.EscapeDataString(command.NationalCode)}",
            insurancer: command.Insurancer,
            cancellationToken);

        if (inquiryResult.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            inquiryResult = await SendGetAsync<List<SepidRadisanInquiryDto>>(
                InquiryPath,
                token,
                query: $"NationalCode={Uri.EscapeDataString(command.NationalCode)}",
                insurancer: command.Insurancer,
                cancellationToken);
        }

        if (!inquiryResult.Success)
            return OperationResult.Failure(inquiryResult.ErrorMessage ?? "استعلام بیمه‌شده ناموفق بود");

        var inquiries = inquiryResult.Data ?? [];
        var policies = new List<SepidRadisanPolicyDto>();

        foreach (var inquiry in inquiries)
        {
            var policyResult = await SendGetAsync<SepidRadisanPolicyInfoDto>(
                PolicyInfoPath,
                token,
                query: $"PolicyId={inquiry.PolicyId}",
                insurancer: command.Insurancer,
                cancellationToken);

            if (policyResult.StatusCode == HttpStatusCode.Unauthorized)
            {
                token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
                if (string.IsNullOrWhiteSpace(token))
                    return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

                policyResult = await SendGetAsync<SepidRadisanPolicyInfoDto>(
                    PolicyInfoPath,
                    token,
                    query: $"PolicyId={inquiry.PolicyId}",
                    insurancer: command.Insurancer,
                    cancellationToken);
            }

            if (!policyResult.Success || policyResult.Data == null)
                return OperationResult.Failure(policyResult.ErrorMessage ?? "دریافت اطلاعات معرفی‌نامه ناموفق بود");

            var info = policyResult.Data;
            policies.Add(new SepidRadisanPolicyDto
            {
                PolicyId = info.PolicyId,
                PolicyFullNo = info.PolicyFullNo,
                InsuredId = inquiry.InsuredId,
                PolicyHolderName = info.PolicyHolderName,
                PolicyStartDate = info.PolicyStartDate,
                PolicyEndDate = info.PolicyEndDate,
            });
        }

        return OperationResult<List<SepidRadisanPolicyDto>>.Success(policies);
    }

    public async Task<OperationResult> GetInsuredInfoAsync(
        SepidRadisanInsuredInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.InsuredId))
            return OperationResult.Failure("شناسه بیمه‌شده الزامی است");

        if (string.IsNullOrWhiteSpace(command.PolicyId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var query = $"insuredId={Uri.EscapeDataString(command.InsuredId)}&policyId={Uri.EscapeDataString(command.PolicyId)}";
        var result = await SendGetAsync<SepidRadisanInsuredInfoDto>(
            InsuredInfoPath,
            token,
            query,
            command.Insurancer,
            cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendGetAsync<SepidRadisanInsuredInfoDto>(
                InsuredInfoPath,
                token,
                query,
                command.Insurancer,
                cancellationToken);
        }

        if (!result.Success || result.Data == null)
            return OperationResult.Failure(result.ErrorMessage ?? "دریافت اطلاعات بیمه‌شده ناموفق بود");

        return OperationResult<SepidRadisanInsuredInfoDto>.Success(result.Data);
    }

    public async Task<OperationResult> PreCheckIntroductionAsync(
        SepidRadisanPreCheckIntroductionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.InsuredId))
            return OperationResult.Failure("شناسه بیمه‌شده الزامی است");

        if (string.IsNullOrWhiteSpace(command.PolicyId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        if (command.ServiceInsures == null || command.ServiceInsures.Count == 0)
            return OperationResult.Failure("لیست خدمات الزامی است");

        if (!long.TryParse(command.PolicyId, out var policyId))
            return OperationResult.Failure("شناسه معرفی‌نامه نامعتبر است");

        if (!long.TryParse(command.InsuredId, out var insuredId))
            return OperationResult.Failure("شناسه بیمه‌شده نامعتبر است");

        var requestBody = new SepidRadisanPreCheckIntroRequestDto
        {
            PolicyId = policyId,
            InsuredId = insuredId,
            DoctorMedicalSystemCode = command.DoctorMedicalSystemCode,
            MedicalSpecialty = command.MedicalSpecialty,
            Services = command.ServiceInsures.ToList(),
        };

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendPostJsonBytesAsync<SepidRadisanPreCheckIntroResponseDto>(
            PreCheckIntroductionPath,
            token,
            requestBody,
            command.Insurancer,
            cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendPostJsonBytesAsync<SepidRadisanPreCheckIntroResponseDto>(
                PreCheckIntroductionPath,
                token,
                requestBody,
                command.Insurancer,
                cancellationToken);
        }

        if (!result.Success || result.Data == null)
            return OperationResult.Failure(result.ErrorMessage ?? "پیش‌بررسی معرفی‌نامه ناموفق بود");

        return OperationResult<SepidRadisanPreCheckIntroResponseDto>.Success(result.Data);
    }

    public async Task<OperationResult> CreateIntroductionAsync(
        SepidRadisanCreateIntroductionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.InsuredId))
            return OperationResult.Failure("شناسه بیمه‌شده الزامی است");

        if (string.IsNullOrWhiteSpace(command.PolicyId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        if (string.IsNullOrWhiteSpace(command.Checksum))
            return OperationResult.Failure("checksum الزامی است");

        if (command.ServiceInsures == null || command.ServiceInsures.Count == 0)
            return OperationResult.Failure("لیست خدمات الزامی است");

        if (!long.TryParse(command.PolicyId, out var policyId))
            return OperationResult.Failure("شناسه معرفی‌نامه نامعتبر است");

        if (!long.TryParse(command.InsuredId, out var insuredId))
            return OperationResult.Failure("شناسه بیمه‌شده نامعتبر است");

        var requestBody = new SepidRadisanCreateIntroductionRequestDto
        {
            Checksum = command.Checksum,
            DoctorName = command.DoctorName ?? string.Empty,
            InsuredId = insuredId,
            PolicyId = policyId,
            DoctorMedicalSystemCode = command.DoctorCode ?? string.Empty,
            ReceptionDate = command.ReceptionDate ?? string.Empty,
            DoctorOrderDate = command.DoctorOrderDate ?? string.Empty,
            MedicalSpecialty = command.MedicalSpecialty ?? string.Empty,
            Discharged = false,
            Services = command.ServiceInsures.ToList(),
        };

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendPostJsonBytesAsync<SepidRadisanCreateIntroductionResponseDto>(
            CreateIntroductionPath,
            token,
            requestBody,
            command.Insurancer,
            cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendPostJsonBytesAsync<SepidRadisanCreateIntroductionResponseDto>(
                CreateIntroductionPath,
                token,
                requestBody,
                command.Insurancer,
                cancellationToken);
        }

        if (!result.Success || result.Data == null)
            return OperationResult.Failure(result.ErrorMessage ?? "ثبت معرفی‌نامه ناموفق بود");

        return OperationResult<SepidRadisanCreateIntroductionResponseDto>.Success(result.Data);
    }

    public async Task<OperationResult> PutDischargeAsync(
        SepidRadisanPutDischargeCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.IntroductionId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        if (string.IsNullOrWhiteSpace(command.DischargeDate))
            return OperationResult.Failure("تاریخ ترخیص الزامی است");

        var requestBody = new SepidRadisanDischargeRequestDto
        {
            DischargeDate = command.DischargeDate,
        };

        var path = $"api/Introductions/{command.IntroductionId}/discharge";
        var extraHeaders = new Dictionary<string, string>
        {
            ["introductionId"] = command.IntroductionId,
        };

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendPutJsonBytesAsync(path, token, requestBody, command.Insurancer, extraHeaders, cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendPutJsonBytesAsync(path, token, requestBody, command.Insurancer, extraHeaders, cancellationToken);
        }

        if (!result.Success)
            return OperationResult.Failure(result.ErrorMessage ?? "ترخیص معرفی‌نامه ناموفق بود");

        var response = OperationResult<string>.Success(result.Data ?? string.Empty);
        response.Message = "عملیات با موفقیت انجام شد.";
        return response;
    }

    public async Task<OperationResult> DeleteIntroductionAsync(
        SepidRadisanDeleteIntroductionCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.IntroductionId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        var requestBody = new SepidRadisanDeleteIntroductionRequestDto
        {
            IntroductionId = command.IntroductionId,
        };

        var path = $"api/Introductions/Void?introductionId={Uri.EscapeDataString(command.IntroductionId)}";

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendPutJsonBytesAsync(path, token, requestBody, command.Insurancer, extraHeaders: null, cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendPutJsonBytesAsync(path, token, requestBody, command.Insurancer, extraHeaders: null, cancellationToken);
        }

        if (!result.Success)
            return OperationResult.Failure(result.ErrorMessage ?? "حذف معرفی‌نامه ناموفق بود");

        return OperationResult<string>.Success("عملیات حذف با موفقیت انجام شد");
    }

    public async Task<OperationResult> AppendAttachmentAsync(
        SepidRadisanAppendAttachmentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrWhiteSpace(command.Password))
            return OperationResult.Failure("نام کاربری و رمز عبور الزامی است");

        if (string.IsNullOrWhiteSpace(command.Insurancer))
            return OperationResult.Failure("بیمه‌گر الزامی است");

        if (string.IsNullOrWhiteSpace(command.IntroductionId))
            return OperationResult.Failure("شناسه معرفی‌نامه الزامی است");

        if (string.IsNullOrWhiteSpace(command.Caption))
            return OperationResult.Failure("caption الزامی است");

        if (command.Files == null || command.Files.Length == 0)
            return OperationResult.Failure("فایل ضمیمه الزامی است");

        var path = $"api/Introductions/{command.IntroductionId}/append-attachment";

        var token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: false, cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

        var result = await SendMultipartPostAsync(path, token, command.Insurancer, command.Caption, command.Files, cancellationToken);

        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await EnsureTokenAsync(command.Username, command.Password, forceRefresh: true, cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return OperationResult.Failure("دریافت توکن SepidRadisan ناموفق بود");

            result = await SendMultipartPostAsync(path, token, command.Insurancer, command.Caption, command.Files, cancellationToken);
        }

        if (!result.Success)
            return OperationResult.Failure(result.ErrorMessage ?? "ارسال ضمیمه ناموفق بود");

        return OperationResult<string>.Success(result.Data ?? string.Empty);
    }

    private async Task<string?> EnsureTokenAsync(
        string username,
        string password,
        bool forceRefresh,
        CancellationToken cancellationToken)
    {
        if (!forceRefresh)
        {
            var cached = await repository.GetByUsernameAsync(username, cancellationToken);
            if (cached != null
                && !string.IsNullOrWhiteSpace(cached.SessionId)
                && cached.ExpireDateSup.HasValue
                && cached.ExpireDateSup.Value > DateTime.Now)
            {
                return cached.SessionId;
            }
        }

        var tokenResult = await FetchTokenFromApiAsync(username, password, cancellationToken);
        if (!tokenResult.Success || string.IsNullOrWhiteSpace(tokenResult.Data?.Token))
            return null;

        var expireDate = tokenResult.Data.ExpireDateTime ?? DateTime.Now.AddHours(6);
        await repository.SaveTokenAsync(username, tokenResult.Data.Token, expireDate, cancellationToken);
        return tokenResult.Data.Token;
    }

    private async Task<SendResult<SepidRadisanTokenResponseDto>> FetchTokenFromApiAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(_settings.TokenPath, query: null);
            var json = JsonConvert.SerializeObject(new SepidRadisanTokenRequestDto
            {
                Username = username,
                Password = password,
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = content;

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<SepidRadisanTokenResponseDto>.Fail(
                    string.IsNullOrWhiteSpace(body) ? $"خطای HTTP {(int)response.StatusCode}" : body,
                    response.StatusCode);
            }

            var data = JsonConvert.DeserializeObject<SepidRadisanTokenResponseDto>(body);
            if (data == null || string.IsNullOrWhiteSpace(data.Token))
                return SendResult<SepidRadisanTokenResponseDto>.Fail("پاسخ توکن SepidRadisan نامعتبر است", response.StatusCode);

            return SendResult<SepidRadisanTokenResponseDto>.Ok(data, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<SepidRadisanTokenResponseDto>.Fail(ex.Message, null);
        }
    }

    private async Task<SendResult<T>> SendGetAsync<T>(
        string path,
        string token,
        string? query,
        string? insurancer,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(path, query);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (!string.IsNullOrWhiteSpace(insurancer))
                request.Headers.TryAddWithoutValidation("insurancer", insurancer);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<T>.Fail(
                    string.IsNullOrWhiteSpace(body) ? $"خطای HTTP {(int)response.StatusCode}" : body,
                    response.StatusCode);
            }

            var data = JsonConvert.DeserializeObject<T>(body);
            if (data == null)
                return SendResult<T>.Fail("پاسخ سرویس SepidRadisan نامعتبر است", response.StatusCode);

            return SendResult<T>.Ok(data, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<T>.Fail(ex.Message, null);
        }
    }

    private async Task<SendResult<T>> SendPostJsonBytesAsync<T>(
        string path,
        string token,
        object body,
        string? insurancer,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(path, query: null);
            var json = JsonConvert.SerializeObject(body, PreCheckRequestJsonSettings);
            var buffer = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(buffer);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            if (!string.IsNullOrWhiteSpace(insurancer))
                request.Headers.TryAddWithoutValidation("insurancer", insurancer);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<T>.Fail(
                    ExtractApiErrorMessage(responseBody, response.StatusCode),
                    response.StatusCode);
            }

            var data = JsonConvert.DeserializeObject<T>(responseBody);
            if (data == null)
                return SendResult<T>.Fail("پاسخ سرویس SepidRadisan نامعتبر است", response.StatusCode);

            return SendResult<T>.Ok(data, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<T>.Fail(ex.Message, null);
        }
    }

    private async Task<SendResult<string>> SendPutJsonBytesAsync(
        string path,
        string token,
        object body,
        string? insurancer,
        IReadOnlyDictionary<string, string>? extraHeaders,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(path, query: null);
            var json = JsonConvert.SerializeObject(body, PreCheckRequestJsonSettings);
            var buffer = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(buffer);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            if (!string.IsNullOrWhiteSpace(insurancer))
                request.Headers.TryAddWithoutValidation("insurancer", insurancer);

            if (extraHeaders != null)
            {
                foreach (var (key, value) in extraHeaders)
                    request.Headers.TryAddWithoutValidation(key, value);
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<string>.Fail(
                    ExtractApiErrorMessage(responseBody, response.StatusCode),
                    response.StatusCode);
            }

            return SendResult<string>.Ok(responseBody, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<string>.Fail(ex.Message, null);
        }
    }

    private async Task<SendResult<string>> SendMultipartPostAsync(
        string path,
        string token,
        string insurancer,
        string caption,
        SepidRadisanUploadFileDto[] files,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(path, query: null);

            using var formData = new MultipartFormDataContent();
            foreach (var file in files)
                formData.Add(new ByteArrayContent(file.Content), "files", file.FileName);

            formData.Add(new StringContent(caption), "caption");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.TryAddWithoutValidation("insurancer", insurancer);
            request.Content = formData;

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<string>.Fail(
                    ExtractApiErrorMessage(responseBody, response.StatusCode),
                    response.StatusCode);
            }

            return SendResult<string>.Ok(responseBody, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<string>.Fail(ex.Message, null);
        }
    }

    private static string ExtractApiErrorMessage(string body, HttpStatusCode statusCode)
    {
        if (string.IsNullOrWhiteSpace(body))
            return $"خطای HTTP {(int)statusCode}";

        try
        {
            var error = JsonConvert.DeserializeObject<SepidRadisanApiErrorResponseDto>(body);
            if (!string.IsNullOrWhiteSpace(error?.Message))
                return error.Message;
        }
        catch
        {
            // ignored
        }

        return body;
    }

    private async Task<SendResult<T>> SendPostAsync<T>(
        string path,
        string token,
        object body,
        string? insurancer,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildUrl(path, query: null);
            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            if (!string.IsNullOrWhiteSpace(insurancer))
                request.Headers.TryAddWithoutValidation("insurancer", insurancer);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return SendResult<T>.Fail(
                    string.IsNullOrWhiteSpace(responseBody) ? $"خطای HTTP {(int)response.StatusCode}" : responseBody,
                    response.StatusCode);
            }

            var data = JsonConvert.DeserializeObject<T>(responseBody);
            if (data == null)
                return SendResult<T>.Fail("پاسخ سرویس SepidRadisan نامعتبر است", response.StatusCode);

            return SendResult<T>.Ok(data, response.StatusCode);
        }
        catch (Exception ex)
        {
            return SendResult<T>.Fail(ex.Message, null);
        }
    }

    private string BuildUrl(string path, string? query)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var relativePath = path.TrimStart('/');
        var url = $"{baseUrl}/{relativePath}";

        if (!string.IsNullOrWhiteSpace(query))
            url = $"{url}?{query}";

        return url;
    }

    private sealed class SendResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }
        public HttpStatusCode? StatusCode { get; init; }

        public static SendResult<T> Ok(T data, HttpStatusCode? statusCode)
            => new() { Success = true, Data = data, StatusCode = statusCode };

        public static SendResult<T> Fail(string message, HttpStatusCode? statusCode)
            => new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }
}
