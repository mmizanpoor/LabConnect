using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Repositories
{
    public class TestECLRepository : ITestECL
    {
        private readonly PTNServiceDbContext _context;

        public TestECLRepository(PTNServiceDbContext context)
        {
            _context = context;
        }

        public List<TestECLViewModel> GetTestECLs()
        {
            var tests = _context.TestECL.Where(x => x.bitActive).ToList();

            var viewModels = MapperConfig.MapperConfig.Mapper.Map<List<TestECLViewModel>>(tests);

            return viewModels;
        }

        public List<TestECLViewModel> GetTestJoze3()
        {
            var tests = _context.TestJoze3.Where(x => x.bitActive).ToList();

            var viewModels = MapperConfig.MapperConfig.Mapper.Map<List<TestECLViewModel>>(tests);

            return viewModels;
        }
    }
}
