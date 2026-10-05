namespace LabConnectPortal.Api.Domain.Enums;

/// <summary>
/// Company legal document kinds. At most one record may exist per type.
/// </summary>
public enum CompanyRegulationType
{
    /// <summary>قوانین مقررات شرکت</summary>
    RulesAndRegulations = 1,

    /// <summary>سیاست خط مشی شرکت</summary>
    CompanyPolicy = 2,
}
