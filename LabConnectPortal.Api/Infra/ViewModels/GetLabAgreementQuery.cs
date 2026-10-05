namespace LabConnectPortal.Infra.ViewModels
{
    public class GetLabAgreementQuery
    {
        public bool IsReceived { get; set; }
        public int PrimaryLabCodeNew { get; set; }
        public DateTime StartDateTime { get; set; }
        public List<int?>? LaboratoryAgreementStates { get; set; }
    }
}
