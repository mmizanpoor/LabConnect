namespace LabConnectPortal.Api.Infrastructure.ViewModels.CenterProfile;

public class InitPayChargeCommand
{
    /// <summary>تعداد درخواستی برای حالت Fixed و Range.</summary>
    public int? Count { get; set; }

    /// <summary>شناسه ردیف قیمت بسته برای حالت Package.</summary>
    public int? PriceId { get; set; }
}
