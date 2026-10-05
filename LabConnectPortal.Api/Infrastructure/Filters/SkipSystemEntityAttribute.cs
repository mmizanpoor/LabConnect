namespace LabConnectPortal.Api.Infrastructure.Filters;

/// <summary>
/// Marks an endpoint/controller as exempt from SystemEntity header validation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class SkipSystemEntityAttribute : Attribute;
