using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class TestECLService(ITestECLRepository repository) : ITestECLService
{
    public Task<OperationResult> GetTestsECLAsync()
    {
        var items = repository.GetTestECLs();
        return Task.FromResult<OperationResult>(OperationResult<IReadOnlyList<TestECLViewModel>>.Success(items));
    }

    public Task<OperationResult> GetTestsJoze3Async()
    {
        var items = repository.GetTestJoze3();
        return Task.FromResult<OperationResult>(OperationResult<IReadOnlyList<TestECLViewModel>>.Success(items));
    }
}
