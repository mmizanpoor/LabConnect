namespace LabConnectPortal.Api.Infrastructure.ViewModels.Legacy
{
    public class GetActiveContractLaboratoryQuery
    {
        public int LabCode { get; set; }
    }

    public class ActiveContractLaboratoryResult
    {
        public ActiveContractLaboratoryResult(List<int> labCodes)
        {
            LabCodes = labCodes;
        }
        public List<int> LabCodes { get; set; }
    }
}
