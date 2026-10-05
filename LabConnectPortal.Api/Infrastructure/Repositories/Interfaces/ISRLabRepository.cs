using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces
{
    public interface ISRLabRepository
    {
        Task<SRLabNameViewModel?> GetSRLabName(int labcode);
        Task<List<SRLabNameViewModel>> GetSRLabNameList(List<int> labcodes);
        Task<List<SRLabNameViewModel>> GetSourceSenderLabName(int targetLabCode);
        Task<List<SRLabNameViewModel>> GetTargetLabName(int sourceLabCode);
        Task<List<SRLabNameViewModel>> GetAllSRLabs(int labCode,bool incoming);
    }
}

