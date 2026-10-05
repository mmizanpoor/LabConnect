using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class LabPermissionActionAttribute(LabPermissionAction action) : Attribute
{
    public LabPermissionAction Action { get; } = action;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipLabPermissionAttribute : Attribute;
