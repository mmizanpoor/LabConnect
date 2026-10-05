using LabConnectPortal.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LabConnectPortal.Api.Infrastructure.Auth;

public sealed class UserTypeAuthorizationRequirement : IAuthorizationRequirement
{
    public UserTypeAuthorizationRequirement(IReadOnlyList<UserType> allowedUserTypes)
    {
        AllowedUserTypes = allowedUserTypes;
    }

    public IReadOnlyList<UserType> AllowedUserTypes { get; }
}

public sealed class UserTypeAuthorizationHandler : AuthorizationHandler<UserTypeAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserTypeAuthorizationRequirement requirement)
    {
        var userTypeClaim = context.User.FindFirst("userType")?.Value;
        if (string.IsNullOrEmpty(userTypeClaim))
            return Task.CompletedTask;

        if (!Enum.TryParse<UserType>(userTypeClaim, out var userType))
            return Task.CompletedTask;

        if (requirement.AllowedUserTypes.Contains(userType))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
