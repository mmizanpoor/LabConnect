using LabConnectPortal.Api.Domain.Enums;

namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class UpdateCenterProfileCommand
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int? EstablishedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public EmployeeCountRange? EmployeeCount { get; set; }
    public string? EconomicCode { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>کد OTP هنگام تغییر موبایل یا ایمیل</summary>
    public string? Code { get; set; }

    /// <summary>Email یا Mobile</summary>
    public string? Channel { get; set; }
}
