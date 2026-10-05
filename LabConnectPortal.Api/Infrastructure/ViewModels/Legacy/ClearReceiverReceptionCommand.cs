namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

public class ClearReceiverReceptionItem
{
    public long Id { get; set; }
    public int SourceLabId { get; set; }
    public string SourceReceptId { get; set; } = string.Empty;
    public int TargetLabId { get; set; }
}

public class ClearReceiverReceptionCommand
{
    public List<ClearReceiverReceptionItem> Items { get; set; } = [];
}
