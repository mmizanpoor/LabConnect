namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class GetLabAgreementQuery
    {
        public AgreementDirection AgreementDirection { get; set; }
        public int PrimaryLabCodeNew { get; set; }
        public DateTime StartDateTime { get; set; }
        public List<int?>? LaboratoryAgreementStates { get; set; }
    }

    public enum AgreementDirection
    {
        Received = 1,
        Sent = 2,
    }
}

