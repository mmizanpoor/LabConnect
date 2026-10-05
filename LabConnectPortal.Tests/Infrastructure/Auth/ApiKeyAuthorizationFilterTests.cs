using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Auth;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.ApiKey;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace LabConnectPortal.Tests.Infrastructure.Auth;

public class ApiKeyAuthorizationFilterTests
{
    [Fact]
    public async Task OnAuthorizationAsync_AcceptsAuthorizationApiKeyHeader_AndStoresContext()
    {
        var expected = new ApiKeyAuthorizationDto
        {
            ApiKeyId = Guid.NewGuid(),
            CenterProfileId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };
        var service = new FakeApiKeyService(
            OperationResult<ApiKeyAuthorizationDto>.Success(expected));
        var filter = new ApiKeyAuthorizationFilter(ApiKeyPermission.Add, service);
        var context = CreateContext();
        context.HttpContext.Request.Headers.Authorization = "ApiKey sample-key";

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal("sample-key", service.RawKey);
        Assert.Equal(ApiKeyPermission.Add, service.Permission);
        Assert.Same(expected, context.HttpContext.Items[ApiKeyAuthorizationFilter.ContextItemKey]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_ReturnsForbiddenOperationResult_WhenAccessIsDenied()
    {
        const string message = "شما دسترسی ندارید. برای دریافت دسترسی با شرکت تماس حاصل نمایید.";
        var service = new FakeApiKeyService(
            OperationResult<ApiKeyAuthorizationDto>.Failure(message));
        var filter = new ApiKeyAuthorizationFilter(ApiKeyPermission.View, service);
        var context = CreateContext();

        await filter.OnAuthorizationAsync(context);

        var objectResult = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        var operationResult = Assert.IsType<OperationResult>(objectResult.Value);
        Assert.False(operationResult.Status);
        Assert.Equal(message, operationResult.Message);
    }

    private static AuthorizationFilterContext CreateContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, []);
    }

    private sealed class FakeApiKeyService(
        OperationResult<ApiKeyAuthorizationDto> authorizationResult) : IApiKeyService
    {
        public string? RawKey { get; private set; }
        public ApiKeyPermission Permission { get; private set; }

        public Task<OperationResult<ApiKeyAuthorizationDto>> AuthorizeAsync(
            string? rawKey,
            ApiKeyPermission permission)
        {
            RawKey = rawKey;
            Permission = permission;
            return Task.FromResult(authorizationResult);
        }

        public Task<OperationResult<List<ApiKeyDto>>> GetMineAsync(Guid userId) => throw new NotSupportedException();
        public Task<OperationResult<ApiKeyDto>> CreateAsync(Guid userId, CreateApiKeyCommand command) => throw new NotSupportedException();
        public Task<OperationResult<ApiKeyDto>> UpdateMineAsync(Guid userId, UpdateApiKeyCommand command) => throw new NotSupportedException();
        public Task<OperationResult> DeleteMineAsync(Guid userId, Guid id) => throw new NotSupportedException();
        public Task<OperationResult<List<ApiKeyDto>>> GetAllAsync() => throw new NotSupportedException();
        public Task<OperationResult<ApiKeyDto>> UpdateAsync(UpdateApiKeyCommand command) => throw new NotSupportedException();
        public Task<OperationResult> DeleteAsync(Guid id) => throw new NotSupportedException();
    }
}
