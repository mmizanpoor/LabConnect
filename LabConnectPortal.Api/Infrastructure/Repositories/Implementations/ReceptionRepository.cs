using System.Text.RegularExpressions;
using LabConnectPortal.Api.Domain.Entities;
using LabConnectPortal.Api.Domain.Enums;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Enums;
using LabConnectPortal.Api.Infrastructure.Repositories.Interfaces;
using LabConnectPortal.Api.Infrastructure.Utils;
using LabConnectPortal.Api.Infrastructure.ViewModels.Dashboard;
using LabConnectPortal.Api.Infrastructure.ViewModels.Legacy;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Infrastructure.Repositories.Implementations
{
    public class ReceptionRepository : SamanehRepository<ReceptionNew>, IReceptionRepository
    {
        private readonly SamanehDbContext _context;

        public ReceptionRepository(SamanehDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddRangeReceptions(List<ReceptionNew> receptions)
        {
            _context.ReceptionNew.AddRange(receptions);
        }

        public void AddRangeReceptTestNew(List<ReceptTestNew> ReceptTestsNew)
        {
            _context.ReceptTestNew.AddRange(ReceptTestsNew);
        }

        public void AddRangeReceptTestP(List<ReceptTestP> ReceptTestsPs)
        {
            _context.ReceptTestP.AddRange(ReceptTestsPs);
        }

        public void AddRangeReceptTestS(List<ReceptTestS> ReceptTestSs)
        {
            _context.ReceptTestS.AddRange(ReceptTestSs);
        }

        private IQueryable<ReceptionNew> BaseQuery()
        {
            return _context.ReceptionNew
                .Include(x => x.ReceptTests)
                .Include(x => x.ReceptTestPs)
                .Include(x => x.ReceptTestSs)
                .AsQueryable();
        }

        public List<ReceptTestComparisonResult> GetReceptTestComparisonResults(TestPriceComparisonFilterQuery query)
        {
            var dbQuery = _context.ReceptTestNew.AsQueryable();

            dbQuery = dbQuery.Where(x => query.Ids.Any(id => id == x.ID));

            var result = dbQuery.Select(x => new ReceptTestComparisonResult()
            {
                Id = x.ID,
                SourceReceptId = x.chrSourceReceptId,
                SourceTestName = x.vchSourceTestName,
                TargetApprovePrice = x.TargetApprovePrice
            }).ToList();

            return result;
        }

        public List<ReceptionViewModel> GetReceptionsByFilter(ReceiveReceptionGroupFilterQuery filter)
        {
            var receptionList = new List<ReceptionViewModel>();

            var receptions = BaseQuery().Where(x => x.intTargetLabId == filter.LabCode);
            //x.ReceptTests.Any(rt => rt.intTargetLabId == filter.LabCode) ||
            //x.ReceptTestPs.Any(rt => rt.intTargetLabId == filter.LabCode) ||
            //x.ReceptTestSs.Any(rt => rt.intTargetLabId == filter.LabCode));


            if (!string.IsNullOrEmpty(filter.ReceptNoSender))
                receptions = receptions.Where(x => x.chrSourceReceptId == filter.ReceptNoSender);
            else
            {
                string fromDate = CustomConverter.MiladiDateToStrShamsi(filter.FromDate) + "00:00";
                string toDate = CustomConverter.MiladiDateToStrShamsi(filter.ToDate) + "23:59";
                receptions = receptions.Where(x => x.chrSourceSendReceptDate.CompareTo(fromDate) >= 0 && x.chrSourceSendReceptDate.CompareTo(toDate) <= 0);

                if (filter.SourceLabCodes != null)
                {
                    var sourceLabCodes = filter.SourceLabCodes.ToList();

                    receptions = receptions.Where(x => sourceLabCodes.Contains(x.intSourceLabId));
                }

                if (filter.IsReject)
                    receptions = receptions.Where(x => x.ReceptTests != null && x.ReceptTests.Any(rt => rt.bitTargetRejected));
                else if (filter.IsReception == true)
                    receptions = receptions.Where(x =>
                        x.ReceptTests != null && x.ReceptTests.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)));
                else if (filter.IsReception == false)
                    receptions = receptions.Where(x =>
                        x.ReceptTests != null && x.ReceptTests.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)));
                // IsReception == null → no acceptance-status filter (all)
            }
            var receptionEntities = receptions.ToList();
            if (receptionEntities.Count > 0)
                receptionList = ConvertToViewModel(receptionEntities);

            var result = new List<ReceptionViewModel>();

            foreach (var item in receptionList)
            {
                IEnumerable<ReceptTestViewModel> query = item.ReceptTests;

                if (filter.IsReject)
                {
                    query = query.Where(x => x.TargetRejected);
                }
                else
                {
                    query = query.Where(x => !x.TargetRejected);
                    if (filter.IsReception == true)
                        query = query.Where(x => !string.IsNullOrEmpty(x.TargetReceptId));
                    else if (filter.IsReception == false)
                        query = query.Where(x => string.IsNullOrEmpty(x.TargetReceptId));
                }

                var tests = query.ToList();
                if (tests.Any())
                {
                    item.ReceptTests = tests;
                    result.Add(item);
                }
            }
            return result;
        }

        public void UpdateTargetReceptId(List<UpdateTargetReceptIdCommand> command)
        {
            var reception = command.FirstOrDefault(x => !string.IsNullOrEmpty(x.TargetReceptId));

            if (reception != null)
            {
                _context.ReceptionNew
                .Where(x => x.intSourceLabId == reception.SourceLabId && x.intTargetLabId == reception.TargetLabId && x.chrSourceReceptId.Trim() == reception.SourceReceptId)
                .ExecuteUpdate(b => b
                .SetProperty(t => t.chrTargetReceptId, reception.TargetReceptId)
                );
            }

            foreach (var item in command)
            {
                var receptTestAny = _context.ReceptTestNew.Any(x => x.ID == item.Id && x.chrSourceReceptId == item.SourceReceptId);
                if (receptTestAny)
                {
                    _context.ReceptTestNew
                        .Where(x => x.ID == item.Id)
                        .ExecuteUpdate(b => b
                        .SetProperty(t => t.chrTargetReceptId, item.TargetReceptId)
                        .SetProperty(t => t.intTargetLabId, item.TargetLabId)
                        .SetProperty(t => t.vchSourceCPN, item.TargetCPN)
                        .SetProperty(t => t.chrTargetReceptDate, item.TargetReceptDate)
                        .SetProperty(t => t.TargetApprovePrice, item.TargetApprovePrice)
                        .SetProperty(t => t.TargetReportingDateTime, item.TargetReportingDateTime)
                        .SetProperty(t => t.Tracking, item.Tracking)
                        .SetProperty(t => t.TargetSendAutoState, item.TargetSendAutoState)
                        );
                }

                var receptTestPAny = _context.ReceptTestP.Any(x => x.ID == item.Id && x.chrSourceReceptId == item.SourceReceptId);
                if (receptTestPAny)
                {
                    _context.ReceptTestP
                       .Where(x => x.ID == item.Id)
                       .ExecuteUpdate(b => b
                       .SetProperty(t => t.chrTargetReceptId, item.TargetReceptId)
                       .SetProperty(t => t.intTargetLabId, item.TargetLabId)
                       .SetProperty(t => t.vchSourceCPN, item.TargetCPN)
                       .SetProperty(t => t.chrTargetReceptDate, item.TargetReceptDate)
                       .SetProperty(t => t.TargetApprovePrice, item.TargetApprovePrice)
                       .SetProperty(t => t.TargetReportingDateTime, item.TargetReportingDateTime)
                       .SetProperty(t => t.Tracking, item.Tracking)
                       );
                }

                var receptTestSAny = _context.ReceptTestS.Any(x => x.ID == item.Id && x.chrSourceReceptId == item.SourceReceptId);
                if (receptTestSAny)
                {
                    _context.ReceptTestS
                       .Where(x => x.ID == item.Id)
                       .ExecuteUpdate(b => b
                       .SetProperty(t => t.chrTargetReceptId, item.TargetReceptId)
                       .SetProperty(t => t.intTargetLabId, item.TargetLabId)
                       .SetProperty(t => t.vchSourceCPN, item.TargetCPN)
                       .SetProperty(t => t.chrTargetReceptDate, item.TargetReceptDate)
                       .SetProperty(t => t.TargetApprovePrice, item.TargetApprovePrice)
                       .SetProperty(t => t.TargetReportingDateTime, item.TargetReportingDateTime)
                       .SetProperty(t => t.Tracking, item.Tracking)
                       );
                }
            }
        }

        public int ClearReceiverReceptions(List<ClearReceiverReceptionItem> items)
        {
            if (items == null || items.Count == 0)
                return 0;

            var eligibleIds = items
                .Where(x => x.Id > 0 && x.SourceLabId > 0 && x.TargetLabId > 0 && !string.IsNullOrWhiteSpace(x.SourceReceptId))
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            if (eligibleIds.Count == 0)
                return 0;

            // Only clear tests that still have empty result.
            var testsToClear = _context.ReceptTestNew
                .AsNoTracking()
                .Where(x => eligibleIds.Contains(x.ID) && string.IsNullOrEmpty(x.vchResult))
                .Select(x => new
                {
                    x.ID,
                    x.intSourceLabId,
                    SourceReceptId = x.chrSourceReceptId,
                    x.intTargetLabId,
                })
                .ToList();

            if (testsToClear.Count == 0)
                return 0;

            var testIds = testsToClear.Select(x => x.ID).ToList();
            _context.ReceptTestNew
                .Where(x => testIds.Contains(x.ID))
                .ExecuteUpdate(b => b
                    .SetProperty(t => t.chrTargetReceptId, (string?)null)
                    .SetProperty(t => t.chrTargetReceptDate, (string?)null));

            var receptionKeys = testsToClear
                .Select(x => new
                {
                    x.intSourceLabId,
                    SourceReceptId = (x.SourceReceptId ?? string.Empty).Trim(),
                    x.intTargetLabId,
                })
                .Where(x => !string.IsNullOrEmpty(x.SourceReceptId))
                .Distinct()
                .ToList();

            foreach (var key in receptionKeys)
            {
                _context.ReceptionNew
                    .Where(x =>
                        x.intSourceLabId == key.intSourceLabId &&
                        x.intTargetLabId == key.intTargetLabId &&
                        x.chrSourceReceptId.Trim() == key.SourceReceptId)
                    .ExecuteUpdate(b => b.SetProperty(t => t.chrTargetReceptId, (string?)null));
            }

            return testsToClear.Count;
        }

        public List<AddReceptTestNewResponse>? UpdateResult(List<UpdateReportingItemsCommand> reportingItems)
        {
            var result = new List<AddReceptTestNewResponse>();

            foreach (var item in reportingItems)
            {
                LabReceiverRangeDetail? rangeDetail = new LabReceiverRangeDetail();
                string strRange = string.Empty;

                if (item.RangeDetail != null)
                {
                    rangeDetail = _context.LabReceiverRangeDetail.FirstOrDefault(x => x.TargetLabId == item.TargetLabId && x.TargetRangeDetailId == item.RangeDetail.TargetRangeDetailId);

                    if (rangeDetail == null)
                    {
                        rangeDetail = ConvertToRangeDetail(item.RangeDetail);
                        AddRangeDetail(rangeDetail);
                    }
                    else
                    {
                        rangeDetail.MinWarningValue = item.RangeDetail.MinWarningValue;
                        rangeDetail.MinPossibleValue = item.RangeDetail.MinPossibleValue;
                        rangeDetail.MinNormalValue = item.RangeDetail.MinNormalValue;
                        rangeDetail.MinCriticalValue = item.RangeDetail.MinCriticalValue;

                        rangeDetail.MaxWarningValue = item.RangeDetail.MaxWarningValue;
                        rangeDetail.MaxPossibleValue = item.RangeDetail.MaxPossibleValue;
                        rangeDetail.MaxNormalValue = item.RangeDetail.MaxNormalValue;
                        rangeDetail.MaxCriticalValue = item.RangeDetail.MaxCriticalValue;

                        rangeDetail.WarningUpperLimitValue = item.RangeDetail.WarningUpperLimitValue;
                        rangeDetail.WarningUpperLimitText = item.RangeDetail.WarningUpperLimitText;
                        rangeDetail.WarningLowerLimitValue = item.RangeDetail.WarningLowerLimitValue;
                        rangeDetail.WarningLowerLimitText = item.RangeDetail.WarningLowerLimitText;
                        rangeDetail.BorderLineText = item.RangeDetail.BorderLineText;

                        rangeDetail.CriticalUpperLimitValue = item.RangeDetail.CriticalUpperLimitValue;
                        rangeDetail.CriticalUpperLimitText = item.RangeDetail.CriticalUpperLimitText;
                        rangeDetail.CriticalLowerLimitValue = item.RangeDetail.CriticalLowerLimitValue;
                        rangeDetail.CriticalLowerLimitText = item.RangeDetail.CriticalLowerLimitText;

                        rangeDetail.FromAge = item.RangeDetail.FromAge;
                        rangeDetail.ToAge = item.RangeDetail.ToAge;
                        rangeDetail.FromAgeScale = item.RangeDetail.FromAgeScale;
                        rangeDetail.IsDeleted = item.RangeDetail.IsDeleted;

                        rangeDetail.UnitDesc = item.RangeDetail.UnitDesc;
                        rangeDetail.KitTitle = item.RangeDetail.KitTitle;
                        rangeDetail.NormalText = item.RangeDetail.NormalText;
                        rangeDetail.Method = item.RangeDetail.Method;
                    }

                    if (!string.IsNullOrEmpty(rangeDetail.NormalText))
                    {
                        string normalText = string.Empty;

                        if (rangeDetail.CriticalLowerLimitValue != 0 && rangeDetail.CriticalUpperLimitValue != 0 && !string.IsNullOrEmpty(rangeDetail.CriticalLowerLimitText) && !string.IsNullOrEmpty(rangeDetail.CriticalUpperLimitText))
                            normalText = $"Desirable : <{rangeDetail.CriticalLowerLimitText}{Environment.NewLine}Moderate risk:{rangeDetail.CriticalLowerLimitValue} - {rangeDetail.CriticalUpperLimitValue}{Environment.NewLine}High : >{rangeDetail.CriticalUpperLimitText}";

                        if (rangeDetail.WarningLowerLimitValue != 0 && rangeDetail.WarningUpperLimitValue != 0 && string.IsNullOrEmpty(normalText) && !string.IsNullOrEmpty(rangeDetail.WarningLowerLimitText) && !string.IsNullOrEmpty(rangeDetail.WarningUpperLimitText))
                            normalText = $"Optimal : <{rangeDetail.WarningLowerLimitText}{Environment.NewLine}Borderline:{rangeDetail.BorderLineText}{Environment.NewLine}High : >{rangeDetail.WarningUpperLimitText}";

                        if (string.IsNullOrEmpty(normalText))
                            normalText = $"{rangeDetail.NormalText}";

                        strRange = ReplaceChar($"{rangeDetail.UnitDesc}UNITMETHODUNIT{rangeDetail.Method}FLDNFLDNFLDN{normalText}FLDNO");
                    }
                    else
                    {
                        string resultStatus = string.Empty;

                        if (!string.IsNullOrEmpty(item.Result) /*&& float.TryParse(item.Result, out float res)*/)
                        {
                            var match = Regex.Match(item.Result, @"^\s*(\d+(?:\.\d+)?)\s*\*?\s*$");

                            if (match.Success && float.TryParse(match.Groups[1].Value, out float res))
                            {

                                if (CheckNormalRangeTypeUse(NormalRangeStatus.Critical, rangeDetail))
                                {
                                    if (res < rangeDetail.MinCriticalValue)
                                        resultStatus = "C";
                                }

                                if (CheckNormalRangeTypeUse(NormalRangeStatus.Warning, rangeDetail) && string.IsNullOrEmpty(resultStatus))
                                {
                                    if (res < rangeDetail.MinWarningValue)
                                        resultStatus = "L";

                                    if (res > rangeDetail.MaxWarningValue)
                                        resultStatus = "H";
                                }

                                if (CheckNormalRangeTypeUse(NormalRangeStatus.Normal, rangeDetail) && string.IsNullOrEmpty(resultStatus))
                                {
                                    if (res >= rangeDetail.MinNormalValue && res <= rangeDetail.MaxNormalValue)
                                        resultStatus = "O";

                                    if (res > rangeDetail.MaxNormalValue && rangeDetail.MinWarningValue == 0 && string.IsNullOrEmpty(resultStatus))
                                        resultStatus = "H";

                                    if (res < rangeDetail.MinNormalValue && rangeDetail.MaxWarningValue == 0 && string.IsNullOrEmpty(resultStatus))
                                        resultStatus = "L";
                                }

                                strRange = ReplaceChar($"{rangeDetail.UnitDesc}UNITMETHODUNIT{rangeDetail.Method}FLDN{rangeDetail.MinNormalValue}FLDN{rangeDetail.MaxNormalValue}FLDN{rangeDetail.MinNormalValue}-{rangeDetail.MaxNormalValue}FLDN{resultStatus}");
                            }
                        }


                    }

                    _context.SaveChanges();
                }

                var sendResultDate = $"{CustomConverter.MiladiDateToStrShamsi(DateTime.Now)}{DateTime.Now.ToString("HH:mm")}";

                if (item.ID > 0)
                {
                    var hasReceptTestWithSameSource = _context.ReceptTestNew.Any(x => x.ID == item.ID && x.chrSourceReceptId == item.SourceReceptId);
                    if (hasReceptTestWithSameSource)
                    {
                        _context.ReceptTestNew.Where(x => x.ID == item.ID)
                        .ExecuteUpdate(b => b
                        .SetProperty(t => t.vchResult, item.Result)
                        .SetProperty(t => t.vchComment, item.Comment)
                        .SetProperty(t => t.chrTargetSendResultDate, sendResultDate)
                        .SetProperty(t => t.Tracking, item.Tracking)
                        .SetProperty(t => t.vchTargetNormalRange, strRange)
                        .SetProperty(t => t.LabReceiverRangeDetailId, rangeDetail != null ? rangeDetail.Id : null)
                        );
                    }

                    var hasReceptTestPWithSameSource = _context.ReceptTestP.Any(x => x.ID == item.ID && x.chrSourceReceptId == item.SourceReceptId);
                    if (hasReceptTestPWithSameSource)
                    {
                        _context.ReceptTestP.Where(x => x.ID == item.ID)
                        .ExecuteUpdate(b => b
                        .SetProperty(t => t.chrTargetSendResultDate, sendResultDate)
                        .SetProperty(t => t.Tracking, item.Tracking)
                        );
                    }

                    var hasReceptTestSWithSameSource = _context.ReceptTestS.Any(x => x.ID == item.ID && x.chrSourceReceptId == item.SourceReceptId);
                    if (hasReceptTestSWithSameSource)
                    {
                        _context.ReceptTestS.Where(x => x.ID == item.ID)
                         .ExecuteUpdate(b => b
                         .SetProperty(t => t.chrTargetSendResultDate, sendResultDate)
                         .SetProperty(t => t.Tracking, item.Tracking)
                         );
                    }
                }
                else
                {
                    var reception = GetReception(item.SourceLabId, item.SourceReceptId, item.TargetLabId).Result;
                    if (reception != null)
                    {
                        var receptTest = new ReceptTestNew()
                        {
                            intSourceLabId = item.SourceLabId,
                            chrSourceReceptId = item.SourceReceptId,
                            chrSourceSendDate = reception.chrSourceSendReceptDate,
                            chrTargetReceptId = reception.chrTargetReceptId,
                            chrTargetReceptDate = reception.chrSourceSendReceptDate,
                            intTargetLabId = item.TargetLabId,
                            vchSourceCPN = item.CPNCode,
                            vchSourceTestName = item?.SourceTestName?.Trim(),
                            bitEmg = item.IsUrgent,
                            vchResult = item.Result,
                            Tracking = item.Tracking,
                            chrTargetSendResultDate = sendResultDate,
                            LabReceiverRangeDetailId = rangeDetail != null ? rangeDetail.Id : null,
                            vchComment = item.Comment,
                            bitAddTestByTarget = true,
                            vchTargetNormalRange = strRange
                        };
                        _context.ReceptTestNew.Add(receptTest);
                        _context.SaveChanges();

                        result.Add(new AddReceptTestNewResponse()
                        {
                            ID = receptTest.ID,
                            ReportingItemId = item.ReportingItemId,
                            AddTestByTarget = receptTest.bitAddTestByTarget,
                            SourceCPN = receptTest.vchSourceCPN,
                            SourceTestName = receptTest.vchSourceTestName ?? ""
                        });
                    }
                }
            }

            return result;
        }

        private bool CheckNormalRangeTypeUse(NormalRangeStatus normalRange, LabReceiverRangeDetail rangeDetail)
        {
            switch (normalRange)
            {
                case NormalRangeStatus.Critical:
                    return rangeDetail.MinCriticalValue != 0 && rangeDetail.MaxCriticalValue != 0;
                case NormalRangeStatus.Warning:
                    return rangeDetail.MinWarningValue != 0 && rangeDetail.MaxWarningValue != 0;
                case NormalRangeStatus.Normal:
                    return rangeDetail.MinNormalValue != 0 && rangeDetail.MaxNormalValue != 0;
                default:
                    return false;
            }
        }

        private string ReplaceChar(string result)
        {
            var temp = result;
            temp = temp.Replace("&", "@ANDSIGN@");
            temp = temp.Replace("?", "@RQUESSIGN@");
            temp = temp.Replace("<", "@LESSSIGN@");
            temp = temp.Replace(">", "@GRATESIGN@");
            temp = temp.Replace("?", "@LQUESSIGN@");
            temp = temp.Replace("\\", "BSLASHSIGN");
            temp = temp.Replace("/", "SLASHSIGN");
            temp = temp.Replace(":", "@SIMISIGN@");
            temp = temp.Replace(";", "@SIMICSIGN@");
            temp = temp.Replace("*", "@STARSIGN@");
            temp = temp.Replace("+", "@PLUSSIGN@");
            temp = temp.Replace("-", "@NEGSIGN@");
            return temp;
        }

        private string ReverseReplaceChar(string result)
        {
            var temp = result;
            temp = temp.Replace("@ANDSIGN@", "&");
            temp = temp.Replace("@RQUESSIGN@", "?");
            temp = temp.Replace("@LESSSIGN@", "<");
            temp = temp.Replace("@GRATESIGN@", ">");
            temp = temp.Replace("@LQUESSIGN@", "?");
            temp = temp.Replace("BSLASHSIGN", "\\");
            temp = temp.Replace("SLASHSIGN", "/");
            temp = temp.Replace("@SIMISIGN@", ":");
            temp = temp.Replace("@SIMICSIGN@", ";");
            temp = temp.Replace("@STARSIGN@", "*");
            temp = temp.Replace("@PLUSSIGN@", "+");
            temp = temp.Replace("@NEGSIGN@", "-");
            return temp;
        }

        public void UpdateResultReceiveDate(List<UpdateReportingItemsCommand> reportingItems)
        {
            foreach (var item in reportingItems)
            {
                var receiveResultDate = $"{CustomConverter.MiladiDateToStrShamsi(DateTime.Now)}{DateTime.Now.ToString("HH:mm")}";
                _context.ReceptTestNew.Where(x => x.ID == item.ID)
                    .ExecuteUpdate(b => b
                    .SetProperty(t => t.chrSourceReceiveResultDate, receiveResultDate)
                    .SetProperty(t => t.Tracking, item.Tracking)
                    );
            }
        }

        public async Task<ReceptionNew?> GetReception(int sourceLabId, string chrSourceReceptId, int targetLabId)
        {
            return await _context.ReceptionNew
                .Include(x => x.ReceptTests)
                .Include(x => x.ReceptTestPs)
                .Include(x => x.ReceptTestSs)
                .Where(x => x.intSourceLabId == sourceLabId && x.chrSourceReceptId.Trim() == chrSourceReceptId.Trim() && x.intTargetLabId == targetLabId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReceptionNew>?> GetReceptionsByKeys(List<ReceptionKeyViewModel> keys)
        {
            var sourceLabIds = keys.Select(x => x.SourceLabId).Distinct().ToList();
            var targetLabIds = keys.Select(x => x.TargetLabId).Distinct().ToList();

            var receptions = await _context.ReceptionNew
                .Include(x => x.ReceptTests)
                .Include(x => x.ReceptTestPs)
                .Include(x => x.ReceptTestSs)
                .Where(x =>
                    sourceLabIds.Contains(x.intSourceLabId) &&
                    targetLabIds.Contains(x.intTargetLabId))
                .ToListAsync();

            var keySet = keys
                .Select(x => (
                    x.SourceLabId,
                    x.TargetLabId,
                    x.SourceReceptId.Trim()))
                .ToHashSet();

            return receptions
                .Where(x => keySet.Contains((
                    x.intSourceLabId,
                    x.intTargetLabId,
                    x.chrSourceReceptId.Trim())))
                .ToList();
        }

        public async Task<ReceptionNew?> GetReceptionByReceptId(int sourceLabId, string chrSourceReceptId)
        {
            chrSourceReceptId = chrSourceReceptId.Trim();
            return await _context.ReceptionNew
                .Include(x => x.ReceptTests)
                .ThenInclude(x => x.LabReceiverRangeDetail)
                .Include(x => x.ReceptTestPs)
                .Include(x => x.ReceptTestSs)
                .Where(x => x.intSourceLabId == sourceLabId && x.chrSourceReceptId.Trim() == chrSourceReceptId)
                .FirstOrDefaultAsync();
        }

        public List<ReceptTestNew> GetReceptTests(int sourceLabId, string chrSourceReceptId, int intTargetLabId, string testName)
        {
            chrSourceReceptId = chrSourceReceptId.Trim();
            testName = testName.Trim();

            return _context.ReceptTestNew.Where(x =>
            x.intSourceLabId == sourceLabId &&
            x.chrSourceReceptId == chrSourceReceptId &&
            x.intTargetLabId == intTargetLabId &&
            x.vchSourceTestName == testName
            ).ToList();
        }

        public List<ReceptionViewModel> GetReportingItemsByFilter(ReceiveGroupReportingItemFilter filter)
        {
            var receptionList = new List<ReceptionViewModel>();

            var receptions = BaseQuery().Where(x =>
                  (x.ReceptTests != null && x.ReceptTests.Any(rt => rt.intSourceLabId == filter.LabCode)) ||
                  (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => rt.intSourceLabId == filter.LabCode)) ||
                  (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => rt.intSourceLabId == filter.LabCode)));

            if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
            {
                string fromDate = CustomConverter.MiladiDateToStrShamsi(Convert.ToDateTime(filter.FromDate)) + "00:00";
                string toDate = CustomConverter.MiladiDateToStrShamsi(Convert.ToDateTime(filter.ToDate)) + "23:59";

                receptions = receptions.Where(x => x.chrSourceSendReceptDate.CompareTo(fromDate) >= 0 && x.chrSourceSendReceptDate.CompareTo(toDate) <= 0);
            }

            if (filter.TargetLabIds != null && filter.TargetLabIds.Any())
            {
                var targetLabIds = filter.TargetLabIds.ToList();
                receptions = receptions.Where(x => (x.ReceptTests != null && x.ReceptTests.Any(rt => targetLabIds.Contains(rt.intTargetLabId))) ||
                                                   (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => rt.intTargetLabId.HasValue && targetLabIds.Contains(rt.intTargetLabId.Value))) ||
                                                   (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => rt.intTargetLabId.HasValue && targetLabIds.Contains(rt.intTargetLabId.Value))));
            }
            if (filter.ReceiveReportType == ReceiveReportType.Rejected)
                receptions = receptions.Where(x => (x.ReceptTests != null && x.ReceptTests.Any(rt => rt.bitTargetRejected)) ||
                                                   (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => rt.bitTargetRejected)) ||
                                                   (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => rt.bitTargetRejected)));
            else if (filter.ReceiveReportType == ReceiveReportType.SourceNotReceiveResult)
                receptions = receptions.Where(x => (x.ReceptTests != null && x.ReceptTests.Any(rt => !string.IsNullOrEmpty(rt.vchResult) && !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))) ||
                                                   (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))) ||
                                                   (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))));
            else if (filter.ReceiveReportType == ReceiveReportType.SourceReceiveResult)
                receptions = receptions.Where(x => (x.ReceptTests != null && x.ReceptTests.Any(rt => !string.IsNullOrEmpty(rt.vchResult) && !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && !string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))) ||
                                                   (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && !string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))) ||
                                                   (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetSendResultDate) && !string.IsNullOrEmpty(rt.chrSourceReceiveResultDate))));

            //receptions = receptions.Where(x => (x.ReceptTests != null && x.ReceptTests.Any(rt => string.IsNullOrEmpty(rt.vchResult))) ||
            //                                              (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => string.IsNullOrEmpty(rt.chrTargetSendResultDate))) ||
            //                                              (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => string.IsNullOrEmpty(rt.chrTargetSendResultDate))));

            var receptionEntities = receptions.ToList();

            return receptionEntities.Count > 0 ? ConvertToViewModel(receptionEntities) : receptionList;
        }

        public void UpdateTracking(long id, int tracking)
        {
            _context.ReceptTestNew.Where(x => x.ID == id)
            .ExecuteUpdate(b => b.SetProperty(t => t.Tracking, tracking));
        }

        public List<ReceptionNew> GetReceptionsByFilterReceptTest(List<ReceptTestNew> receptTests)
        {
            var receptions = _context.ReceptionNew.AsEnumerable().Where(x => receptTests.Any(r => r.chrSourceReceptId == x.chrSourceReceptId && r.intSourceLabId == x.intSourceLabId)).AsQueryable();

            return receptions.ToList();
        }

        public List<ReceptionNew> GetReceptionsByReceptTest(int sourceLabId)
        {
            var receptions = _context.ReceptionNew.Where(x => x.intSourceLabId == sourceLabId).AsQueryable();


            return receptions.ToList();
        }

        public void UpdateReceptionId(List<UpdateReceptionVM> viewModel)
        {
            foreach (var item in viewModel)
            {
                var reception = _context.ReceptionNew.FirstOrDefault(x => x.intSourceLabId == item.intSourceLabId && x.chrSourceReceptId == item.chrSourceReceptId);

                if (reception != null)
                {
                    _context.ReceptionNew.Update(reception);
                }
            }
        }


        public async Task<LabReceiverRangeDetailViewModel?> GetRangeDetail(long targetLabId, long targetRangeDetialId)
        {
            var rangeDetail = await _context.LabReceiverRangeDetail.Where(x => x.TargetLabId == targetLabId && x.TargetRangeDetailId == targetRangeDetialId).FirstOrDefaultAsync();
            return rangeDetail != null ? ConvertToRangeDetailViewModel(rangeDetail) : null;
        }


        public void AddRangeDetail(LabReceiverRangeDetail rangeDetail)
        {
            _context.LabReceiverRangeDetail.Add(rangeDetail);
        }

        public void UpdateRangeDetail(LabReceiverRangeDetail item)
        {
            _context.LabReceiverRangeDetail.Where(x => x.Id == item.Id)
                     .ExecuteUpdate(b => b
                     .SetProperty(t => t.MinWarningValue, item.MinWarningValue)
                     .SetProperty(t => t.MinPossibleValue, item.MinPossibleValue)
                     .SetProperty(t => t.MinCriticalValue, item.MinCriticalValue)
                     .SetProperty(t => t.MaxWarningValue, item.MaxWarningValue)
                     .SetProperty(t => t.BorderLineText, item.BorderLineText)
                     .SetProperty(t => t.FromAge, item.FromAge)
                     .SetProperty(t => t.FromAgeScale, item.FromAgeScale)
                     .SetProperty(t => t.IsDeleted, item.IsDeleted)
                     .SetProperty(t => t.KitTitle, item.KitTitle)
                     .SetProperty(t => t.MaxCriticalValue, item.MaxCriticalValue)
                     .SetProperty(t => t.MaxNormalValue, item.MaxNormalValue)
                     .SetProperty(t => t.MaxPossibleValue, item.MaxPossibleValue)
                     .SetProperty(t => t.MinNormalValue, item.MinNormalValue)
                     .SetProperty(t => t.WarningUpperLimitValue, item.WarningUpperLimitValue)
                     .SetProperty(t => t.WarningUpperLimitText, item.WarningUpperLimitText)
                     .SetProperty(t => t.WarningLowerLimitValue, item.WarningLowerLimitValue)
                     .SetProperty(t => t.WarningLowerLimitText, item.WarningLowerLimitText)
                     .SetProperty(t => t.UnitDesc, item.UnitDesc)
                     .SetProperty(t => t.NormalText, item.NormalText)
                     );

        }

        public void DeleteReceptTest(ReceptTestNew receptTest)
        {
            _context.ReceptTestNew.Remove(receptTest);
        }

        public void RejectTest(RejectReceptTest receptTest)
        {
            string rejectDate = CustomConverter.MiladiDateToStrShamsi(DateTime.Now) + DateTime.Now.ToString("HH:mm");

            _context.ReceptTestNew.Where(x => x.ID == receptTest.Id && x.chrSourceReceptId == receptTest.SourceReceptId)
            .ExecuteUpdate(b => b
               .SetProperty(t => t.bitTargetRejected, true)
               .SetProperty(t => t.vchTestReturnCause, receptTest.TestReturnCause)
               .SetProperty(t => t.chrTargetRejectDate, rejectDate)
            );

            _context.ReceptTestS.Where(x => x.ID == receptTest.Id && x.chrSourceReceptId == receptTest.SourceReceptId)
            .ExecuteUpdate(b => b
               .SetProperty(t => t.bitTargetRejected, true)
               .SetProperty(t => t.vchTestReturnCause, receptTest.TestReturnCause)
               .SetProperty(t => t.chrTargetRejectDate, rejectDate)
            );
        }

        #region  ÇäæÑÊ ãÏá åÇ
        private List<ReceptionViewModel> ConvertToViewModel(List<ReceptionNew> receptions)
        {
            List<ReceptionViewModel> receptionsViewModel = new List<ReceptionViewModel>();

            foreach (var x in receptions)
            {
                var receptionViewModel = new ReceptionViewModel()
                {
                    SourceReceptId = x.chrSourceReceptId,
                    SourceLabId = x.intSourceLabId,
                    Age = x.tinAge,
                    AgeType = x.chrAgeType,
                    DoctorCode = x.DocCode,
                    FirstName = x.vchFname,
                    LastName = x.vchLName,
                    Gender = x.bitSex,
                    IsUrgent = x.bitEmg,
                    Mobile = x.vchMobile,
                    NIC = x.vchNIC,
                    PreviousRecords = x.vchPrevRec,
                    SendFromSite = x.bitFromSite,
                    SourceSendReceptDate = x.chrSourceSendReceptDate,
                    TargetLabId = x.intTargetLabId,
                    TargetReceptId = x.chrTargetReceptId
                };

                var receptTests = new List<ReceptTestViewModel>();

                if (x.ReceptTests != null && x.ReceptTests.Any())
                {
                    receptTests.AddRange(x.ReceptTests.Select(rt => new ReceptTestViewModel()
                    {
                        Id = rt.ID,
                        AddTestByTarget = rt.bitAddTestByTarget,
                        Comment = rt.vchComment,
                        IsUrgent = rt.bitEmg,
                        Result = !string.IsNullOrEmpty(rt.vchResult) ? ReverseReplaceChar(rt.vchResult) : null,
                        SourceCPN = rt.vchSourceCPN,
                        SourceLabId = rt.intSourceLabId,
                        SourceReceiveResultDate = rt.chrSourceReceiveResultDate?.TrimEnd(),
                        SourceReceptId = rt.chrSourceReceptId,
                        SourceSendDate = rt.chrSourceSendDate?.TrimEnd(),
                        SourceTestName = rt.vchSourceTestName,
                        TargetLabId = rt.intTargetLabId,
                        TargetReceptDate = rt.chrTargetReceptDate,
                        TargetReceptId = rt.chrTargetReceptId,
                        TargetRejectDate = rt.chrTargetRejectDate,
                        TargetRejected = rt.bitTargetRejected,
                        TargetSendResultDate = rt.chrTargetSendResultDate,
                        TestReturnCause = rt.vchTestReturnCause,
                        RangeDetail = rt.LabReceiverRangeDetail != null && rt.LabReceiverRangeDetailId.HasValue ? ConvertToRangeDetailViewModel(rt.LabReceiverRangeDetail) : !string.IsNullOrWhiteSpace(rt.vchTargetNormalRange) ? ReversevchNormalRange(rt.vchTargetNormalRange, rt.ID) : null,
                        TargetApprovePrice = rt.TargetApprovePrice,
                        TargetReportingDateTime = rt.TargetReportingDateTime,
                        SourceApprovePrice = rt.SourceApprovePrice,
                        SourceSectionName = rt.SourceSectionName,
                        Tracking = rt.Tracking,
                        TestType = "C",
                        TargetSendAutoState = rt.TargetSendAutoState
                    }).ToList());
                }

                if (x.ReceptTestPs != null && x.ReceptTestPs.Any())
                {
                    receptTests.AddRange(x.ReceptTestPs.Select(rt => new ReceptTestViewModel()
                    {
                        Id = rt.ID,
                        AddTestByTarget = rt.bitAddTestByTarget,
                        Comment = rt.vchComment,
                        IsUrgent = rt.bitEmg.HasValue ? rt.bitEmg.Value : false,
                        imgResult = rt.imgResult,
                        SourceCPN = rt.vchSourceCPN,
                        SourceLabId = rt.intSourceLabId!.Value,
                        SourceReceiveResultDate = rt.chrSourceReceiveResultDate?.TrimEnd(),
                        SourceReceptId = rt.chrSourceReceptId,
                        SourceSendDate = rt.chrSourceSendDate?.TrimEnd(),
                        SourceTestName = rt.vchSourceTestName,
                        TargetLabId = rt.intTargetLabId!.Value,
                        TargetReceptDate = rt.chrTargetReceptDate,
                        TargetReceptId = rt.chrTargetReceptId,
                        TargetRejectDate = rt.chrTargetRejectDate,
                        TargetRejected = rt.bitTargetRejected,
                        TargetSendResultDate = rt.chrTargetSendResultDate,
                        TestReturnCause = rt.vchTestReturnCause,
                        TestType = "P"
                    }).ToList());
                }

                if (x.ReceptTestSs != null && x.ReceptTestSs.Any())
                {
                    receptTests.AddRange(x.ReceptTestSs.Select(rt => new ReceptTestViewModel()
                    {
                        Id = rt.ID,
                        AddTestByTarget = rt.bitAddTestByTarget,
                        Comment = rt.vchComment,
                        IsUrgent = rt.bitEmg.HasValue ? rt.bitEmg.Value : false,
                        imgResult = rt.imgResult,
                        SourceCPN = rt.vchSourceCPN,
                        SourceLabId = rt.intSourceLabId.HasValue ? rt.intSourceLabId.Value : 0,
                        SourceReceiveResultDate = rt.chrSourceReceiveResultDate?.TrimEnd(),
                        SourceReceptId = rt.chrSourceReceptId,
                        SourceSendDate = rt.chrSourceSendDate?.TrimEnd(),
                        SourceTestName = rt.vchSourceTestName,
                        TargetLabId = rt.intTargetLabId.HasValue ? rt.intTargetLabId.Value : 0,
                        TargetReceptDate = rt.chrTargetReceptDate,
                        TargetReceptId = rt.chrTargetReceptId,
                        TargetRejectDate = rt.chrTargetRejectDate,
                        TargetRejected = rt.bitTargetRejected,
                        TargetSendResultDate = rt.chrTargetSendResultDate,
                        TestReturnCause = rt.vchTestReturnCause,
                        TestType = "G"
                    }).ToList());
                }

                receptionViewModel.ReceptTests = receptTests;

                receptionsViewModel.Add(receptionViewModel);
            }

            return receptionsViewModel;
        }

        /// <summary>
        /// ÊÈÏíá äÑãÇá ÑäÌ ÂÒãæä Èå äÑãÇá ÑäÌ Ñæ
        /// </summary>
        /// <returns></returns>
        private LabReceiverRangeDetailViewModel ReversevchNormalRange(string vchTargetNormalRange, long id)
        {
            var targetNormalRange = vchTargetNormalRange;
            var labReceiverMormalRange = new LabReceiverRangeDetailViewModel();
            if (!targetNormalRange.Contains("UNITMETHODUNIT"))
                return labReceiverMormalRange;

            var unitMethod = targetNormalRange.Split(new string[] { "UNITMETHODUNIT" }, StringSplitOptions.None);

            if (unitMethod.Length == 0)
                return labReceiverMormalRange;

            if (!string.IsNullOrWhiteSpace(unitMethod[0]))
                labReceiverMormalRange.UnitDesc = ReverseReplaceChar(unitMethod[0]);

            var methodSplit = unitMethod[1].Split(new string[] { "FLDN" }, StringSplitOptions.None);

            if (!string.IsNullOrWhiteSpace(methodSplit[0]))
                labReceiverMormalRange.Method = ReverseReplaceChar(methodSplit[0]);

            string normalText = string.Empty;
            if (methodSplit[3] != null)
                labReceiverMormalRange.NormalText = ReverseReplaceChar(methodSplit[3]);

            labReceiverMormalRange.Id = id;

            return labReceiverMormalRange;
        }

        /// <summary>
        /// Covert To LabReceiverRangeDetail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public LabReceiverRangeDetailViewModel ConvertToRangeDetailViewModel(LabReceiverRangeDetail model)
        {
            return new LabReceiverRangeDetailViewModel()
            {
                Id = model.Id,
                TargetLabId = model.TargetLabId,
                TargetRangeDetailId = model.TargetRangeDetailId,
                BorderLineText = model.BorderLineText,
                CriticalLowerLimitText = model.CriticalLowerLimitText,
                CriticalLowerLimitValue = model.CriticalLowerLimitValue,
                CriticalUpperLimitText = model.CriticalUpperLimitText,
                CriticalUpperLimitValue = model.CriticalUpperLimitValue,
                FromAge = model.FromAge,
                FromAgeScale = model.FromAgeScale,
                IsDeleted = model.IsDeleted,
                KitTitle = model.KitTitle,
                MaxCriticalValue = model.MaxCriticalValue,
                MaxNormalValue = model.MaxNormalValue,
                MaxPossibleValue = model.MaxPossibleValue,
                MaxWarningValue = model.MaxWarningValue,
                MinCriticalValue = model.MinCriticalValue,
                MinNormalValue = model.MinNormalValue,
                MinPossibleValue = model.MinPossibleValue,
                MinWarningValue = model.MinWarningValue,
                NormalText = model.NormalText,
                RangeSpecificationType = model.RangeSpecificationType,
                ToAge = model.ToAge,
                ToAgeScale = model.ToAgeScale,
                UnitDesc = model.UnitDesc,
                WarningLowerLimitText = model.WarningLowerLimitText,
                WarningLowerLimitValue = model.WarningLowerLimitValue,
                WarningUpperLimitText = model.WarningUpperLimitText,
                WarningUpperLimitValue = model.WarningUpperLimitValue,
                Method = model.Method
            };
        }


        /// <summary>
        /// Covert To LabReceiverRangeDetail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private LabReceiverRangeDetail ConvertToRangeDetail(LabReceiverRangeDetailViewModel model)
        {
            return new LabReceiverRangeDetail()
            {
                Id = model.Id.HasValue ? model.Id.Value : 0,
                TargetLabId = model.TargetLabId,
                TargetRangeDetailId = model.TargetRangeDetailId,
                BorderLineText = model.BorderLineText,
                CriticalLowerLimitText = model.CriticalLowerLimitText,
                CriticalLowerLimitValue = model.CriticalLowerLimitValue,
                CriticalUpperLimitText = model.CriticalUpperLimitText,
                CriticalUpperLimitValue = model.CriticalUpperLimitValue,
                FromAge = model.FromAge,
                FromAgeScale = model.FromAgeScale,
                IsDeleted = model.IsDeleted,
                KitTitle = model.KitTitle,
                MaxCriticalValue = model.MaxCriticalValue,
                MaxNormalValue = model.MaxNormalValue,
                MaxPossibleValue = model.MaxPossibleValue,
                MaxWarningValue = model.MaxWarningValue,
                MinCriticalValue = model.MinCriticalValue,
                MinNormalValue = model.MinNormalValue,
                MinPossibleValue = model.MinPossibleValue,
                MinWarningValue = model.MinWarningValue,
                NormalText = model.NormalText,
                RangeSpecificationType = model.RangeSpecificationType,
                ToAge = model.ToAge,
                ToAgeScale = model.ToAgeScale,
                UnitDesc = model.UnitDesc,
                WarningLowerLimitText = model.WarningLowerLimitText,
                WarningLowerLimitValue = model.WarningLowerLimitValue,
                WarningUpperLimitText = model.WarningUpperLimitText,
                WarningUpperLimitValue = model.WarningUpperLimitValue,
            };
        }

        public ReceptTestNew? GetReceptTestById(long id)
        {
            return _context.ReceptTestNew.Where(x => x.ID == id).FirstOrDefault();
        }

        public List<string?> GetRejectCount(int labcode)
        {
            string fromRejectDate = CustomConverter.MiladiDateToStrShamsi(DateTime.Now) + DateTime.Now.ToString("00:00");
            string toRejectDate = CustomConverter.MiladiDateToStrShamsi(DateTime.Now) + DateTime.Now.ToString("23:59");

            return _context.ReceptTestNew
                .Where(x =>
                 x.intSourceLabId == labcode
                && x.bitTargetRejected
                && !string.IsNullOrWhiteSpace(x.chrTargetRejectDate)
                && x.chrTargetRejectDate.CompareTo(fromRejectDate) >= 0
                && x.chrTargetRejectDate.CompareTo(toRejectDate) <= 0)
                .Select(x => x.chrSourceReceptId)
                .ToList();
        }

        public async Task<AdminReceptionDashboardStatsDto> GetAdminReceptionDashboardStatsAsync(
            int? labCode = null,
            AdminReceptionDashboardSection? section = null)
        {
            var now = DateTime.Now;
            var (dailyFrom, dailyTo) = CustomConverter.GetShamsiDayRange(now);
            var (monthlyFrom, monthlyTo) = CustomConverter.GetShamsiMonthRange(now);
            var (yearlyPopularFrom, yearlyPopularTo) = CustomConverter.GetShamsiPastYearRange(now);

            if (!labCode.HasValue)
            {
                var stats = new AdminReceptionDashboardStatsDto();

                async Task LoadTopLabsAsync()
                {
                    stats.DailyMostUsage = await GetTopLabByReceptionUsageAsync(dailyFrom, dailyTo);
                    stats.MonthlyMostUsage = await GetTopLabByReceptionUsageAsync(monthlyFrom, monthlyTo);
                    stats.DailyMostSentTests = await GetTopLabBySentTestsAsync(dailyFrom, dailyTo);
                    stats.MonthlyMostSentTests = await GetTopLabBySentTestsAsync(monthlyFrom, monthlyTo);
                    stats.DailyMostReceivedTests = await GetTopLabByReceivedTestsAsync(dailyFrom, dailyTo);
                    stats.MonthlyMostReceivedTests = await GetTopLabByReceivedTestsAsync(monthlyFrom, monthlyTo);
                }

                if (!section.HasValue)
                {
                    await LoadTopLabsAsync();
                    stats.DailySentLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: false, dailyFrom, dailyTo);
                    stats.DailyReceivedLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: true, dailyFrom, dailyTo);
                    stats.MonthlySentLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: false, monthlyFrom, monthlyTo);
                    stats.MonthlyReceivedLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: true, monthlyFrom, monthlyTo);
                    stats.MonthlyPopularTests = await GetTopPopularTestsAsync(labCode: null, monthlyFrom, monthlyTo);
                    stats.YearlyPopularTests = await GetTopPopularTestsAsync(labCode: null, yearlyPopularFrom, yearlyPopularTo);
                    return stats;
                }

                switch (section.Value)
                {
                    case AdminReceptionDashboardSection.TopLabs:
                        await LoadTopLabsAsync();
                        break;
                    case AdminReceptionDashboardSection.MonthlyPopularTests:
                        stats.MonthlyPopularTests = await GetTopPopularTestsAsync(labCode: null, monthlyFrom, monthlyTo);
                        break;
                    case AdminReceptionDashboardSection.YearlyPopularTests:
                        stats.YearlyPopularTests = await GetTopPopularTestsAsync(labCode: null, yearlyPopularFrom, yearlyPopularTo);
                        break;
                    case AdminReceptionDashboardSection.DailySentLabs:
                        stats.DailySentLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: false, dailyFrom, dailyTo);
                        break;
                    case AdminReceptionDashboardSection.DailyReceivedLabs:
                        stats.DailyReceivedLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: true, dailyFrom, dailyTo);
                        break;
                    case AdminReceptionDashboardSection.MonthlySentLabs:
                        stats.MonthlySentLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: false, monthlyFrom, monthlyTo);
                        break;
                    case AdminReceptionDashboardSection.MonthlyReceivedLabs:
                        stats.MonthlyReceivedLabs = await GetLabSummaryRowsAsync(labCode: null, isReceived: true, monthlyFrom, monthlyTo);
                        break;
                }

                return stats;
            }

            var sentDaily = await GetLabDirectionStatsAsync(labCode.Value, isReceived: false, dailyFrom, dailyTo);
            var sentMonthly = await GetLabDirectionStatsAsync(labCode.Value, isReceived: false, monthlyFrom, monthlyTo);
            var receivedDaily = await GetLabDirectionStatsAsync(labCode.Value, isReceived: true, dailyFrom, dailyTo);
            var receivedMonthly = await GetLabDirectionStatsAsync(labCode.Value, isReceived: true, monthlyFrom, monthlyTo);

            return new AdminReceptionDashboardStatsDto
            {
                DailyMostUsage = new TopLabMetricDto { LabCode = labCode, Count = sentDaily.UsageCount },
                MonthlyMostUsage = new TopLabMetricDto { LabCode = labCode, Count = sentMonthly.UsageCount },
                DailyMostSentTests = new TopLabMetricDto { LabCode = labCode, Count = sentDaily.TestsCount },
                MonthlyMostSentTests = new TopLabMetricDto { LabCode = labCode, Count = sentMonthly.TestsCount },
                DailyMostReceivedTests = new TopLabMetricDto { LabCode = labCode, Count = receivedDaily.TestsCount },
                MonthlyMostReceivedTests = new TopLabMetricDto { LabCode = labCode, Count = receivedMonthly.TestsCount },
                SentDailyStats = sentDaily,
                SentMonthlyStats = sentMonthly,
                ReceivedDailyStats = receivedDaily,
                ReceivedMonthlyStats = receivedMonthly,
                DailySentLabs = await GetLabSummaryRowsAsync(labCode, isReceived: false, dailyFrom, dailyTo),
                MonthlySentLabs = await GetLabSummaryRowsAsync(labCode, isReceived: false, monthlyFrom, monthlyTo),
                DailyReceivedLabs = await GetLabSummaryRowsAsync(labCode, isReceived: true, dailyFrom, dailyTo),
                MonthlyReceivedLabs = await GetLabSummaryRowsAsync(labCode, isReceived: true, monthlyFrom, monthlyTo),
                MonthlyPopularTests = await GetTopPopularTestsAsync(labCode, monthlyFrom, monthlyTo),
                YearlyPopularTests = await GetTopPopularTestsAsync(labCode, yearlyPopularFrom, yearlyPopularTo),
            };
        }

        private async Task<List<PopularTestMetricDto>> GetTopPopularTestsAsync(
            int? labCode,
            string from,
            string to)
        {
            var tests = FilterTestsByReceptionSendDate(from, to);

            if (labCode.HasValue)
            {
                tests = tests.Where(t =>
                    t.intSourceLabId == labCode.Value || t.intTargetLabId == labCode.Value);
            }

            var raw = await tests
                .Where(t => t.vchSourceCPN != null && t.vchSourceCPN != "")
                .Select(t => new { t.vchSourceCPN, t.vchSourceTestName })
                .ToListAsync();

            var byCpn = raw
                .GroupBy(t => t.vchSourceCPN!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Count = g.Count(),
                    TestName = g
                        .Where(x => !string.IsNullOrWhiteSpace(x.vchSourceTestName))
                        .GroupBy(x => x.vchSourceTestName!.Trim(), StringComparer.OrdinalIgnoreCase)
                        .OrderByDescending(n => n.Count())
                        .Select(n => n.Key)
                        .FirstOrDefault() ?? g.Key,
                });

            return byCpn
                .Select(x => new
                {
                    TestName = ReverseReplaceChar(x.TestName.Trim()),
                    Count = x.Count,
                })
                .GroupBy(x => x.TestName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new PopularTestMetricDto
                {
                    TestName = g.Key,
                    Count = g.Sum(x => x.Count),
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }

        private IQueryable<ReceptionNew> FilterReceptionsBySendDate(string from, string to)
            => _context.ReceptionNew
                .AsNoTracking()
                .Where(x =>
                    x.chrSourceSendReceptDate.CompareTo(from) >= 0
                    && x.chrSourceSendReceptDate.CompareTo(to) <= 0);

        private IQueryable<ReceptTestNew> FilterTestsByReceptionSendDate(string from, string to)
            => _context.ReceptTestNew
                .AsNoTracking()
                .Where(t => _context.ReceptionNew.Any(r =>
                    r.intSourceLabId == t.intSourceLabId
                    && r.chrSourceReceptId == t.chrSourceReceptId
                    && r.intTargetLabId == t.intTargetLabId
                    && r.chrSourceSendReceptDate.CompareTo(from) >= 0
                    && r.chrSourceSendReceptDate.CompareTo(to) <= 0));

        private async Task<TopLabMetricDto> GetTopLabByReceptionUsageAsync(string from, string to)
        {
            var top = await FilterReceptionsBySendDate(from, to)
                .GroupBy(x => x.intSourceLabId)
                .Select(g => new { LabId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return new TopLabMetricDto
            {
                LabCode = top?.LabId,
                Count = top?.Count ?? 0,
            };
        }

        private async Task<TopLabMetricDto> GetTopLabBySentTestsAsync(string from, string to)
        {
            var top = await FilterTestsByReceptionSendDate(from, to)
                .GroupBy(x => x.intSourceLabId)
                .Select(g => new { LabId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return new TopLabMetricDto
            {
                LabCode = top?.LabId,
                Count = top?.Count ?? 0,
            };
        }

        private async Task<TopLabMetricDto> GetTopLabByReceivedTestsAsync(string from, string to)
        {
            var top = await FilterTestsByReceptionSendDate(from, to)
                .GroupBy(x => x.intTargetLabId)
                .Select(g => new { LabId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return new TopLabMetricDto
            {
                LabCode = top?.LabId,
                Count = top?.Count ?? 0,
            };
        }

        private async Task<LabReceptionPeriodStatsDto> GetLabDirectionStatsAsync(
            int labCode,
            bool isReceived,
            string from,
            string to)
        {
            var receptions = FilterReceptionsBySendDate(from, to);
            var tests = FilterTestsByReceptionSendDate(from, to);

            if (isReceived)
            {
                return new LabReceptionPeriodStatsDto
                {
                    UsageCount = await receptions.CountAsync(x => x.intTargetLabId == labCode),
                    TestsCount = await tests.CountAsync(x => x.intTargetLabId == labCode),
                };
            }

            return new LabReceptionPeriodStatsDto
            {
                UsageCount = await receptions.CountAsync(x => x.intSourceLabId == labCode),
                TestsCount = await tests.CountAsync(x => x.intSourceLabId == labCode),
            };
        }

        private async Task<List<LabReceptionSummaryRowDto>> GetLabSummaryRowsAsync(
            int? labCode,
            bool? isReceived,
            string from,
            string to)
        {
            var receptions = FilterReceptionsBySendDate(from, to);
            var tests = FilterTestsByReceptionSendDate(from, to);

            if (!labCode.HasValue)
            {
                if (isReceived == true)
                {
                    var receptionCounts = await receptions
                        .GroupBy(x => x.intTargetLabId)
                        .Select(g => new { LabId = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var testCounts = await tests
                        .GroupBy(x => x.intTargetLabId)
                        .Select(g => new { LabId = g.Key, Count = g.Count() })
                        .ToListAsync();

                    return await BuildSummaryRowsAsync(
                        MapCounts(receptionCounts, x => x.LabId, x => x.Count),
                        MapCounts(testCounts, x => x.LabId, x => x.Count));
                }

                var sentReceptionCounts = await receptions
                    .GroupBy(x => x.intSourceLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var sentTestCounts = await tests
                    .GroupBy(x => x.intSourceLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                return await BuildSummaryRowsAsync(
                    MapCounts(sentReceptionCounts, x => x.LabId, x => x.Count),
                    MapCounts(sentTestCounts, x => x.LabId, x => x.Count));
            }

            if (isReceived == true)
            {
                var receptionCounts = await receptions
                    .Where(x => x.intTargetLabId == labCode.Value)
                    .GroupBy(x => x.intSourceLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var testCounts = await tests
                    .Where(x => x.intTargetLabId == labCode.Value)
                    .GroupBy(x => x.intSourceLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                return await BuildSummaryRowsAsync(
                    MapCounts(receptionCounts, x => x.LabId, x => x.Count),
                    MapCounts(testCounts, x => x.LabId, x => x.Count));
            }

            {
                var receptionCounts = await receptions
                    .Where(x => x.intSourceLabId == labCode.Value)
                    .GroupBy(x => x.intTargetLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                var testCounts = await tests
                    .Where(x => x.intSourceLabId == labCode.Value)
                    .GroupBy(x => x.intTargetLabId)
                    .Select(g => new { LabId = g.Key, Count = g.Count() })
                    .ToListAsync();

                return await BuildSummaryRowsAsync(
                    MapCounts(receptionCounts, x => x.LabId, x => x.Count),
                    MapCounts(testCounts, x => x.LabId, x => x.Count));
            }
        }

        private async Task<List<LabReceptionSummaryRowDto>> BuildSummaryRowsAsync(
            List<(int LabId, int Count)> receptionCounts,
            List<(int LabId, int Count)> testCounts)
        {
            var testCountMap = testCounts.ToDictionary(x => x.LabId, x => x.Count);
            var rows = receptionCounts
                .Select(x => new LabReceptionSummaryRowDto
                {
                    LabCode = x.LabId,
                    ReceptionCount = x.Count,
                    TestsCount = testCountMap.GetValueOrDefault(x.LabId),
                })
                .ToList();

            foreach (var testOnly in testCounts.Where(x => rows.All(r => r.LabCode != x.LabId)))
            {
                rows.Add(new LabReceptionSummaryRowDto
                {
                    LabCode = testOnly.LabId,
                    ReceptionCount = 0,
                    TestsCount = testOnly.Count,
                });
            }

            rows = rows
                .OrderByDescending(x => x.ReceptionCount)
                .ThenByDescending(x => x.TestsCount)
                .ToList();

            return rows;
        }

        private static List<(int LabId, int Count)> MapCounts<T>(List<T> items, Func<T, int> labId, Func<T, int> count)
            => items.Select(x => (labId(x), count(x))).ToList();

        #endregion
    }
}

