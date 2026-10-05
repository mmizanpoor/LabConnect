using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Security;

namespace LabConnectPortal.Api.Infrastructure.Services.Interfaces;

public interface ISecuritySmsReportService
{
    Task<OperationResult<SecuritySmsExcelExportResult>> ExportExcelAsync();
}
