namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class GetExistLabAgreementQuery
    {
        public long? Id { get; set; }
        public int PrimaryAgreementLabCodeNew { get; set; }
        public string ContractNumber { get; set; }
    }
}
