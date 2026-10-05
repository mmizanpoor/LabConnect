using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LabConnectPortal.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class TestECLController(ITestECLService testECLService) : ControllerBase
{
    [HttpGet("GetTestsECL")]
    public async Task<OperationResult> GetTestsECL()
        => await testECLService.GetTestsECLAsync();

    [HttpGet("GetTestsJoze3")]
    public async Task<OperationResult> GetTestsJoze3()
        => await testECLService.GetTestsJoze3Async();
}
