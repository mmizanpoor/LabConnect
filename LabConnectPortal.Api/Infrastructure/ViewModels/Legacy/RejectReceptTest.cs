namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class RejectReceptTestCommand
    {
        public List<RejectReceptTest> Items { get; set; }
    }

    public class RejectReceptTest
    {
        public long Id { get; set; }
        public string SourceReceptId { get; set; }
        public string? TestReturnCause { get; set; }
    }
}

