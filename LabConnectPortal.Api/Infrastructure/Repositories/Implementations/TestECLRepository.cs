using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.MapperConfig;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations;

public class TestECLRepository(PtnServiceDbContext context) : ITestECLRepository
{
    private readonly PtnServiceDbContext _context = context;

    public List<TestECLViewModel> GetTestECLs()
    {
        var tests = _context.TestECL.Where(x => x.bitActive).ToList();
        return ApplicationMapper.Mapper.Map<List<TestECLViewModel>>(tests);
    }

    public List<TestECLViewModel> GetTestJoze3()
    {
        var tests = _context.TestJoze3.Where(x => x.bitActive).ToList();
        return ApplicationMapper.Mapper.Map<List<TestECLViewModel>>(tests);
    }
}
