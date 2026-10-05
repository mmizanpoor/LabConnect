using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Interfaces
{
    public interface ISRLab
    {
        Task<SRLabNameViewModel?> GetSRLabName(int labcode);
        Task<List<SRLabNameViewModel>> GetSRLabNameList(List<int> labcodes);
        Task<List<SRLabNameViewModel>> GetSourceSenderLabName(int targetLabCode);
        Task<List<SRLabNameViewModel>> GetTargetLabName(int sourceLabCode);
    }
}
