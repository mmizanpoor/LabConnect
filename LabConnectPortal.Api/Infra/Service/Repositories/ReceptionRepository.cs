using Microsoft.EntityFrameworkCore;
using LabConnectPortal.Domain;
using LabConnectPortal.Infra.Context;
using LabConnectPortal.Infra.Enums;
using LabConnectPortal.Infra.Service.Interfaces;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Service.Repositories
{
    public class ReceptionRepository : Repository<ReceptionNew>, IReception
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

        public List<ReceptionViewModel> GetReceptionsByFilter(ReceiveReceptionGroupFilterQuery filter)
        {
            var receptionList = new List<ReceptionViewModel>();

            var receptions = BaseQuery().Where(x =>
                   x.ReceptTests.Any(rt => rt.intTargetLabId == filter.LabCode) ||
                   x.ReceptTestPs.Any(rt => rt.intTargetLabId == filter.LabCode) ||
                   x.ReceptTestSs.Any(rt => rt.intTargetLabId == filter.LabCode))
                .AsQueryable();


            if (!string.IsNullOrEmpty(filter.ReceptNoSender))
                receptions = receptions.Where(x => x.chrSourceReceptId == filter.ReceptNoSender);
            else
            {
                string fromDate = CustomConverter.MiladiDateToStrShamsi(filter.FromDate) + "00:00";
                string toDate = CustomConverter.MiladiDateToStrShamsi(filter.ToDate) + "23:59";
                receptions = receptions.Where(x => x.chrSourceSendReceptDate.CompareTo(fromDate) >= 0 && x.chrSourceSendReceptDate.CompareTo(toDate) <= 0);

                if (filter.SourceLabCodes != null)
                    receptions = receptions.Where(x => filter.SourceLabCodes.Any(l => l == x.intSourceLabId));

                if (filter.IsReject)
                    receptions = receptions.Where(x => x.ReceptTests.Any(rt => rt.bitTargetRejected));
                else if (filter.IsReception == true)
                    receptions = receptions.Where(x =>
                        x.ReceptTests.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestSs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestPs.Any(rt => !string.IsNullOrEmpty(rt.chrTargetReceptId)));
                else if (filter.IsReception == false)
                    receptions = receptions.Where(x =>
                        x.ReceptTests.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestSs.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)) ||
                        x.ReceptTestPs.Any(rt => string.IsNullOrEmpty(rt.chrTargetReceptId)));
            }

            if (receptions.Count() > 0)
                receptionList = ConvertToViewModel(receptions.ToList());

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

                        if (!string.IsNullOrEmpty(item.Result) && float.TryParse(item.Result, out float res))
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
            temp = temp.Replace("¿", "@LQUESSIGN@");
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
            temp = temp.Replace("@LQUESSIGN@", "¿");
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

        public async Task<ReceptionNew?> GetReceptionByReceptId(int sourceLabId, string chrSourceReceptId)
        {
            return await _context.ReceptionNew
                .Include(x => x.ReceptTests)
                .ThenInclude(x => x.LabReceiverRangeDetail)
                .Include(x => x.ReceptTestPs)
                .Include(x => x.ReceptTestSs)
                .Where(x => x.intSourceLabId == sourceLabId && x.chrSourceReceptId.Trim() == chrSourceReceptId.Trim())
                .FirstOrDefaultAsync();
        }

        public List<ReceptTestNew> GetReceptTests(int sourceLabId, string chrSourceReceptId, int intTargetLabId, string testName)
        {
            return _context.ReceptTestNew.Where(x => x.intSourceLabId == sourceLabId && x.chrSourceReceptId.Trim() == chrSourceReceptId.Trim() && x.intTargetLabId == intTargetLabId && x.vchSourceTestName.Trim() == testName.Trim()).ToList();
        }

        public List<ReceptionViewModel> GetReportingItemsByFilter(ReceiveGroupReportingItemFilter filter)
        {
            var receptionList = new List<ReceptionViewModel>();

            var receptions = BaseQuery().Where(x =>
                  (x.ReceptTests != null && x.ReceptTests.Any(rt => rt.intSourceLabId == filter.LabCode)) ||
                  (x.ReceptTestPs != null && x.ReceptTestPs.Any(rt => rt.intSourceLabId == filter.LabCode)) ||
                  (x.ReceptTestSs != null && x.ReceptTestSs.Any(rt => rt.intSourceLabId == filter.LabCode)))
             .AsQueryable();

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

            if (receptions.Count() > 0)
                receptionList = ConvertToViewModel(receptions.ToList());

            return receptionList;
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

        #region  کانورت مدل ها
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
                        SourceLabId = rt.intSourceLabId.Value,
                        SourceReceiveResultDate = rt.chrSourceReceiveResultDate?.TrimEnd(),
                        SourceReceptId = rt.chrSourceReceptId,
                        SourceSendDate = rt.chrSourceSendDate?.TrimEnd(),
                        SourceTestName = rt.vchSourceTestName,
                        TargetLabId = rt.intTargetLabId.Value,
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
        /// تبدیل نرمال رنج آزمون به نرمال رنج پرو
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

        #endregion
    }
}
