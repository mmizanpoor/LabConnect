using LabConnectPortal.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace LabConnectPortal.Api.Infrastructure.Auth;

/// <summary>
/// Resolves policies named <c>UserType:Type1,Type2</c> into a requirement with those allowed types.
/// </summary>
public sealed class UserTypeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith("UserType:", StringComparison.Ordinal))
        {
            var typeNames = policyName["UserType:".Length..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var types = new List<UserType>();
            foreach (var name in typeNames)
            {
                if (!Enum.TryParse<UserType>(name, out var userType))
                    return Task.FromResult<AuthorizationPolicy?>(null);
                types.Add(userType);
            }

            if (types.Count == 0)
                return Task.FromResult<AuthorizationPolicy?>(null);

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new UserTypeAuthorizationRequirement(types))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
