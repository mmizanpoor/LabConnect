using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Auth;
using LabConnectPortal.Api.Infrastructure.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class LoginAttemptService(LabConnectDbContext context) : ILoginAttemptService
{
    public async Task RecordAsync(RecordLoginAttemptCommand command)
    {
        try
        {
            context.UserLoginLogs.Add(new UserLoginLog
            {
                UserId = command.UserId,
                LoginMethod = command.LoginMethod,
                Success = command.Success,
                Username = Truncate(command.Username, 100),
                MobileNumber = Truncate(command.MobileNumber, 20),
                IpAddress = Truncate(command.IpAddress, 45),
                UserAgent = Truncate(command.UserAgent, 500),
                FailureReason = Truncate(command.FailureReason, 300),
                CreatedAt = DateTime.UtcNow.ToLocalTime(),
            });
            await context.SaveChangesAsync();
        }
        catch
        {
            // Never block authentication because of logging failures.
        }
    }

    public Task<OperationResult<PagedResult<UserLoginLogDto>>> GetMineAsync(
        Guid userId,
        GetLoginAttemptsQuery query)
        => QueryAsync(query, userId);

    public Task<OperationResult<PagedResult<UserLoginLogDto>>> GetAllAsync(GetLoginAttemptsQuery query)
        => QueryAsync(query, scopeUserId: null);

    private async Task<OperationResult<PagedResult<UserLoginLogDto>>> QueryAsync(
        GetLoginAttemptsQuery query,
        Guid? scopeUserId)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var dbQuery = context.UserLoginLogs.AsNoTracking().AsQueryable();

        if (scopeUserId.HasValue)
            dbQuery = dbQuery.Where(x => x.UserId == scopeUserId.Value);

        if (query.From.HasValue)
            dbQuery = dbQuery.Where(x => x.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            dbQuery = dbQuery.Where(x => x.CreatedAt <= query.To.Value);

        if (query.Success.HasValue)
            dbQuery = dbQuery.Where(x => x.Success == query.Success.Value);

        if (query.LoginMethod.HasValue)
            dbQuery = dbQuery.Where(x => x.LoginMethod == query.LoginMethod.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            dbQuery = dbQuery.Where(x =>
                (x.Username != null && x.Username.Contains(term)) ||
                (x.MobileNumber != null && x.MobileNumber.Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync();
        var rows = await dbQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.LoginMethod,
                x.Success,
                x.Username,
                x.MobileNumber,
                x.IpAddress,
                x.FailureReason,
                x.CreatedAt,
                UserUsername = x.User != null ? x.User.Username : null,
                UserFirstName = x.User != null ? x.User.FirstName : null,
                UserLastName = x.User != null ? x.User.LastName : null,
                UserMobile = x.User != null ? x.User.MobileNumber : null,
            })
            .ToListAsync();

        var items = rows.Select(x =>
        {
            var fullName = $"{x.UserFirstName ?? ""} {x.UserLastName ?? ""}".Trim();
            var displayName = !string.IsNullOrWhiteSpace(fullName)
                ? fullName
                : !string.IsNullOrWhiteSpace(x.UserUsername)
                    ? x.UserUsername
                    : x.UserMobile ?? x.Username ?? x.MobileNumber;

            return new UserLoginLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                DisplayName = displayName,
                LoginMethod = x.LoginMethod,
                Success = x.Success,
                Username = x.Username,
                MobileNumber = x.MobileNumber,
                IpAddress = x.IpAddress,
                FailureReason = x.FailureReason,
                CreatedAt = x.CreatedAt,
            };
        }).ToList();

        return OperationResult<PagedResult<UserLoginLogDto>>.Success(new PagedResult<UserLoginLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        });
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
