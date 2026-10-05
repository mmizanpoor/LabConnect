using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Interfaces
{
    public interface ITestECLRepository
    {
        List<TestECLViewModel> GetTestECLs();
        List<TestECLViewModel> GetTestJoze3();
    }
}

