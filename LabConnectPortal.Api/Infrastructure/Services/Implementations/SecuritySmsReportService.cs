using ClosedXML.Excel;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Services.Interfaces;
using LabConnectPortal.Api.Infrastructure.ViewModels;
using LabConnectPortal.Api.Infrastructure.ViewModels.Security;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Services.Implementations;

public class SecuritySmsReportService(
    SamanehDbContext samanehDbContext,
    PtnServiceDbContext ptnServiceDbContext) : ISecuritySmsReportService
{
    public async Task<OperationResult<SecuritySmsExcelExportResult>> ExportExcelAsync()
    {
        var securities = await samanehDbContext.Securities
            .AsNoTracking()
            .Where(x => x.chrSenderType == "L")
            .Select(s => new
            {
                s.intLabCode,
                s.intLabCodeNew,
                s.vchLockData,
                s.chrSenderType,
                s.chrExpDate,
                s.intSmsCount,
            })
            .ToListAsync();

        var walletsByLabCode = (await ptnServiceDbContext.SmsAccounts
             .AsNoTracking()
             .Select(a => new { a.LabCode, a.CenterTag, a.Wallet })
             .ToListAsync())
             .GroupBy(a => (a.LabCode, a.CenterTag))
             .ToDictionary(g => g.Key, g => g.Max(x => x.Wallet));

        var rows = securities
            .Select(s => new SecuritySmsWalletExportDto
            {
                intLabCode = s.intLabCode,
                intLabCodeNew = s.intLabCodeNew,
                vchLockData = s.vchLockData,
                chrSenderType = s.chrSenderType ?? string.Empty,
                chrExpDate = s.chrExpDate ?? string.Empty,
                intSmsCount = s.intSmsCount,
                Wallet = walletsByLabCode.TryGetValue((s.intLabCodeNew, s.chrSenderType), out var wallet) ? wallet : 0,
            })
            .OrderBy(r => r.intLabCode)
            .ToList();

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("SecuritySmsWallet");

        sheet.Cell(1, 1).Value = "intLabCode";
        sheet.Cell(1, 2).Value = "intLabCodeNew";
        sheet.Cell(1, 3).Value = "vchLockData";
        sheet.Cell(1, 4).Value = "chrSenderType";
        sheet.Cell(1, 5).Value = "chrExpDate";
        sheet.Cell(1, 6).Value = "intSmsCount";
        sheet.Cell(1, 7).Value = "Wallet";
        sheet.Range(1, 1, 1, 7).Style.Font.Bold = true;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var excelRow = i + 2;
            sheet.Cell(excelRow, 1).Value = row.intLabCode;
            sheet.Cell(excelRow, 2).Value = row.intLabCodeNew;
            sheet.Cell(excelRow, 3).Value = row.vchLockData ?? string.Empty;
            sheet.Cell(excelRow, 4).Value = row.chrSenderType;
            sheet.Cell(excelRow, 5).Value = row.chrExpDate;
            sheet.Cell(excelRow, 6).Value = row.intSmsCount;
            sheet.Cell(excelRow, 7).Value = row.Wallet;
        }

        sheet.Columns().AdjustToContents();

        var fileName = $"security-sms-wallet-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var exportDir = Path.Combine(documentsPath, "LabConnectPortal");
        Directory.CreateDirectory(exportDir);
        var savedPath = Path.Combine(exportDir, fileName);

        workbook.SaveAs(savedPath);

        return OperationResult<SecuritySmsExcelExportResult>.Success(new SecuritySmsExcelExportResult
        {
            FileName = fileName,
            SavedPath = savedPath,
        });
    }
}
