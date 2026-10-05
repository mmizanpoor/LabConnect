using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Extensions;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Notification;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUserRepository userRepository,
    HttpClient httpClient,
    IOptions<ExternalNotificationSettings> settings,
    ILogger<NotificationService> logger) : INotificationService
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    private static readonly Regex SensitiveJsonValueRegex = new(
        """(?i)("(?:password|passwd|pwd|token|access[_-]?token|refresh[_-]?token|authorization|api[_-]?key|client[_-]?secret|secret|bearer)"\s*:\s*")([^"]*)(")""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SensitiveQueryRegex = new(
        """(?i)([?&](?:password|passwd|pwd|token|access_token|refresh_token|authorization|api_key|client_secret|secret|bearer)=)([^&]*)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ExternalNotificationSettings _settings = settings.Value;

    public async Task CreateAsync(Guid userId, string title, string message, NotificationType type)
    {
        notificationRepository.Add(new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        });
        await notificationRepository.SaveChangesAsync();
    }

    public async Task<OperationResult> SendExternalNotificationAsync(
        NotificationCommandBase command,
        IEnumerable<Guid> targetUserIds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return OperationResult.Failure("عنوان پیغام الزامی است");

        if (string.IsNullOrWhiteSpace(command.Message))
            return OperationResult.Failure("متن پیغام الزامی است");

        var userIdList = targetUserIds.Distinct().ToList();
        if (userIdList.Count == 0)
            return OperationResult.Failure("حداقل یک کاربر هدف باید مشخص شود");

        var labCodeNews = new HashSet<int>();
        foreach (var userId in userIdList)
        {
            var user = await userRepository.GetWithRolesAsync(userId);
            if (user?.GetLabCodeNew() is int labCodeNew && labCodeNew > 0)
                labCodeNews.Add(labCodeNew);
        }

        if (labCodeNews.Count == 0)
            return OperationResult.Failure("کاربران هدف فاقد کد آزمایشگاه معتبر هستند");

        command.TargetUsers = await BuildTargetUsersFromLabCodesAsync(labCodeNews, cancellationToken);

        if (command.TargetUsers.Count == 0)
            return OperationResult.Failure("هیچ مشتری برای کاربران هدف یافت نشد");

        return await SendNotificationAsync(command, cancellationToken);
    }

    public async Task<OperationResult> SendExternalNotificationByLabCodesAsync(
        NotificationCommandBase command,
        IEnumerable<int> labCodeNews,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
            return OperationResult.Failure("عنوان پیغام الزامی است");

        if (string.IsNullOrWhiteSpace(command.Message))
            return OperationResult.Failure("متن پیغام الزامی است");

        var codes = labCodeNews.Where(c => c > 0).Distinct().ToList();
        if (codes.Count == 0)
            return OperationResult.Failure("حداقل یک کد آزمایشگاه باید مشخص شود");

        command.TargetUsers = await BuildTargetUsersFromLabCodesAsync(codes, cancellationToken);

        if (command.TargetUsers.Count == 0)
            return OperationResult.Failure("هیچ مشتری برای کدهای آزمایشگاه یافت نشد");

        return await SendNotificationAsync(command, cancellationToken);
    }

    public async Task<OperationResult<string>> ResolveCustomerLabNameAsync(
        int labCodeNew,
        CancellationToken cancellationToken = default)
    {
        if (labCodeNew <= 0)
            return OperationResult<string>.Failure("کد آزمایشگاه نامعتبر است");

        var customersResult = await SearchCustomersAsync(labCodeNew, cancellationToken);
        if (!customersResult.Status || customersResult.Data == null || customersResult.Data.Count == 0)
            return OperationResult<string>.Failure(customersResult.Message ?? "مشتری یافت نشد");

        var customer = customersResult.Data.FirstOrDefault(c => c.Code == labCodeNew)
            ?? customersResult.Data.First();

        return OperationResult<string>.Success(customer.LabName);
    }

    private async Task<List<IdTitleDto>> BuildTargetUsersFromLabCodesAsync(
        IEnumerable<int> labCodeNews,
        CancellationToken cancellationToken)
    {
        var targetUsers = new List<IdTitleDto>();
        foreach (var labCodeNew in labCodeNews.Distinct())
        {
            var customersResult = await SearchCustomersAsync(labCodeNew, cancellationToken);
            if (!customersResult.Status || customersResult.Data == null || customersResult.Data.Count == 0)
                continue;

            var customer = customersResult.Data.FirstOrDefault(c => c.Code == labCodeNew)
                ?? customersResult.Data.First();

            targetUsers.Add(new IdTitleDto
            {
                Id = customer.Id,
                Title = customer.LabName,
            });
        }

        return targetUsers
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();
    }

    public async Task<OperationResult<List<NotificationDto>>> GetMyNotificationsAsync(Guid userId)
    {
        var items = await notificationRepository.GetByUserIdAsync(userId);
        return OperationResult<List<NotificationDto>>.Success(items.Select(MapToDto).ToList());
    }

    public async Task<OperationResult<int>> GetUnreadCountAsync(Guid userId)
    {
        var count = await notificationRepository.GetUnreadCountAsync(userId);
        return OperationResult<int>.Success(count);
    }

    public async Task<OperationResult> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await notificationRepository.GetByIdForUserAsync(notificationId, userId);
        if (notification == null)
            return OperationResult.Failure("پیغام یافت نشد");

        notification.IsRead = true;
        notificationRepository.Update(notification);
        return await notificationRepository.SaveChangesAsync();
    }

    private async Task<OperationResult<List<CustomerDto>>> SearchCustomersAsync(
        int labCodeNew,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = BuildSearchUrl(labCodeNew);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return OperationResult<List<CustomerDto>>.Failure(
                    ExtractExternalErrorMessage(responseBody, (int)response.StatusCode));
            }

            var customers = DeserializeCustomerList(responseBody);
            if (customers == null)
                return OperationResult<List<CustomerDto>>.Failure("پاسخ سرویس جستجوی مشتری نامعتبر است");

            return OperationResult<List<CustomerDto>>.Success(customers);
        }
        catch (Exception ex)
        {
            return OperationResult<List<CustomerDto>>.Failure(ex.Message);
        }
    }

    private static List<CustomerDto>? DeserializeCustomerList(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return [];

        var token = JToken.Parse(body);
        if (token.Type == JTokenType.Array)
            return token.ToObject<List<CustomerDto>>();

        var dataToken = token["data"];
        if (dataToken?.Type == JTokenType.Array)
            return dataToken.ToObject<List<CustomerDto>>();

        var wrapped = JsonConvert.DeserializeObject<OperationResult<List<CustomerDto>>>(body);
        return wrapped?.Data;
    }

    public async Task<OperationResult> SendNotificationAsync(
        NotificationCommandBase command,
        CancellationToken cancellationToken = default)
    {
        command.TargetUsers ??= [];
        var payload = BuildExternalNotificationPayload(command);
        var apiUrl = BuildUrl(_settings.SendNotificationPath);
        string? requestBody = null;

        try
        {
            requestBody = JsonConvert.SerializeObject(payload, JsonSettings);
            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var httpError = ExtractExternalErrorMessage(responseBody, (int)response.StatusCode);
                LogSendNotificationError(apiUrl, requestBody, exception: null, errorMessage: httpError);
                return OperationResult<List<IdCodeTitle>>.Failure(httpError);
            }

            return ParseExternalApiResponse(responseBody);
        }
        catch (Exception ex)
        {
            LogSendNotificationError(apiUrl, requestBody, exception: ex, errorMessage: null);
            return OperationResult.Failure(ex.Message);
        }
    }

    private void LogSendNotificationError(
        string apiUrl,
        string? requestBody,
        Exception? exception,
        string? errorMessage)
    {
        TryLog(() =>
        {
            var safeApiUrl = MaskSensitiveData(apiUrl);
            var safeRequestBody = requestBody is null
                ? "[unavailable]"
                : MaskSensitiveData(requestBody);
            var message = exception?.Message ?? errorMessage ?? "Unknown error";
            var stackTrace = exception?.StackTrace;
            var innerException = exception?.InnerException?.ToString();

            if (exception is null)
            {
                logger.LogError(
                    "SendNotificationAsync failed. {ApiUrl} {RequestBody} {ErrorMessage} {StackTrace} {InnerException}",
                    safeApiUrl,
                    safeRequestBody,
                    message,
                    stackTrace,
                    innerException);
                return;
            }

            logger.LogError(
                exception,
                "SendNotificationAsync failed. {ApiUrl} {RequestBody} {ErrorMessage} {StackTrace} {InnerException}",
                safeApiUrl,
                safeRequestBody,
                message,
                stackTrace,
                innerException);
        });
    }

    private static void TryLog(Action logAction)
    {
        try
        {
            logAction();
        }
        catch
        {
            // Logging must never replace or hide the original failure.
        }
    }

    private static string MaskSensitiveData(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        try
        {
            var masked = SensitiveJsonValueRegex.Replace(value, "$1***$3");
            return SensitiveQueryRegex.Replace(masked, "$1***");
        }
        catch
        {
            return value;
        }
    }

    private static ExternalNotificationPayload BuildExternalNotificationPayload(NotificationCommandBase command)
        => new()
        {
            Id = command.Id,
            Title = command.Title,
            Message = command.Message,
            Picture = command.Picture,
            IsActive = command.IsActive,
            HaveToUpdate = command.HaveToUpdate,
            Priority = 1,
            ExpireDate = command.ExpireDate,
            TargetUsers = command.TargetUsers,
            NotificationActions = (command.NotificationActions ?? [])
                .Select(MapExternalAction)
                .ToList(),
        };

    private static ExternalNotificationActionPayload MapExternalAction(NotificationAction action)
        => new()
        {
            NotificationActionType = (int)action.NotificationActionType,
            Label = action.Label,
            Color = action.Color,
            SystemEntityId = action.SystemEntityId,
            Url = action.Url,
            OpenTab = action.OpenTab,
            Controller = action.Controller,
            Action = action.Action,
            Parameter = action.Parameter,
        };

    private static OperationResult ParseExternalApiResponse(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return OperationResult.SuccessResult();

        try
        {
            var token = JToken.Parse(responseBody);

            var statusToken = token["status"] ?? token["Status"];
            if (statusToken?.Type == JTokenType.Boolean)
            {
                if (!statusToken.Value<bool>())
                {
                    var message = token["message"]?.ToString()
                        ?? token["Message"]?.ToString()
                        ?? "ارسال اعلان خارجی ناموفق بود";
                    return OperationResult.Failure(message);
                }

                return OperationResult.SuccessResult();
            }

            var messages = token["messages"]?.ToObject<List<string>>()
                ?? token["Messages"]?.ToObject<List<string>>();
            if (messages != null && messages.Count > 0)
                return OperationResult.Failure(string.Join("; ", messages.Where(m => !string.IsNullOrWhiteSpace(m))));

            // RN.Licensing CommandResult<T> — presence of data (or empty messages) means success.
            return OperationResult.SuccessResult();
        }
        catch (JsonException)
        {
            return OperationResult.SuccessResult();
        }
    }

    private static string ExtractExternalErrorMessage(string? responseBody, int statusCode)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return $"خطای HTTP {statusCode}";

        try
        {
            var token = JToken.Parse(responseBody);
            var messages = token["messages"]?.ToObject<List<string>>()
                ?? token["Messages"]?.ToObject<List<string>>();
            if (messages != null && messages.Count > 0)
                return string.Join("; ", messages.Where(m => !string.IsNullOrWhiteSpace(m)));

            var message = token["message"]?.ToString()
                ?? token["Message"]?.ToString()
                ?? token["title"]?.ToString()
                ?? token["Title"]?.ToString();
            if (!string.IsNullOrWhiteSpace(message))
                return message;
        }
        catch (JsonException)
        {
            // fall through to raw body
        }

        return responseBody;
    }

    private sealed class ExternalNotificationPayload
    {
        public long? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<ExternalNotificationActionPayload> NotificationActions { get; set; } = [];
        public string? Picture { get; set; }
        public bool IsActive { get; set; } = true;
        public bool HaveToUpdate { get; set; }
        public int Priority { get; set; }
        public DateTime? ExpireDate { get; set; }
        public List<IdTitleDto> TargetUsers { get; set; } = [];
    }

    private sealed class ExternalNotificationActionPayload
    {
        public int NotificationActionType { get; set; }
        public string Label { get; set; } = string.Empty;
        public string? Color { get; set; }
        public Guid? SystemEntityId { get; set; }
        public string? Url { get; set; }
        public bool? OpenTab { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Parameter { get; set; }
    }

    private string BuildUrl(string path)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path.TrimStart('/')}";
    }

    private string BuildSearchUrl(int labCodeNew)
    {
        var path = _settings.CustomerSearchPath.TrimStart('/');
        var query = Uri.EscapeDataString(labCodeNew.ToString());
        return $"{_settings.BaseUrl.TrimEnd('/')}/{path}?query={query}";
    }

    private static NotificationDto MapToDto(UserNotification n)
        => new()
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
        };

    public async Task<OperationResult<List<IdCodeTitle>>> GetActiveCustomers()
    {
        try
        {
            var response = await httpClient.GetAsync($"{_settings.BaseUrl.TrimEnd('/')}/Customer/GetLabs");

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var httpError = ExtractExternalErrorMessage(
                    responseBody,
                    (int)response.StatusCode);

                return OperationResult<List<IdCodeTitle>>.Failure(httpError);
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<ExternalCommandResult<List<IdCodeTitle>>>(
            responseBody,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                return OperationResult<List<IdCodeTitle>>.Failure("داده‌ای از سرویس دریافت نشد.");

            return OperationResult<List<IdCodeTitle>>.Success(result.Data ?? []);
        }
        catch (Exception ex)
        {
            return OperationResult<List<IdCodeTitle>>.Failure(ex.Message);
        }
    }
}
