namespace LabConnectPortal.Api.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = "LabConnectPortal-Super-Secret-Key-Min-32-Chars!";
    public string Issuer { get; set; } = "LabConnectPortal";
    public string Audience { get; set; } = "LabConnectPortal.Web";
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
