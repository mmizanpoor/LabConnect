using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Interfaces
{
    public interface ITestECL
    {
        List<TestECLViewModel> GetTestECLs();
        List<TestECLViewModel> GetTestJoze3();
    }
}
