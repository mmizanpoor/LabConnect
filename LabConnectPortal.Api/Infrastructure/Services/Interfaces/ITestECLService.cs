using LabConnectPortal.Api.Infrastructure.ViewModels;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ITestECLService
{
    Task<OperationResult> GetTestsECLAsync();
    Task<OperationResult> GetTestsJoze3Async();
}
