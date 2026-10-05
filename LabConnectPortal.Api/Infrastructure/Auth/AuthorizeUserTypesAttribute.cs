using LabConnectPortal.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LabConnectPortal.Api.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizeUserTypesAttribute : AuthorizeAttribute
{
    public AuthorizeUserTypesAttribute(params UserType[] userTypes)
    {
        Policy = $"UserType:{string.Join(',', userTypes)}";
    }
}
