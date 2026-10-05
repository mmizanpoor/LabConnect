using LabConnectPortal.Domain;
using LabConnectPortal.Infra.ViewModels;

namespace LabConnectPortal.Infra.Utils
{
    public static class ReceptionModelConverter
    {
        #region کانورت مدل ها
        /// <summary>
        /// کانورت لیست به مدل
        /// </summary>
        /// <param name="receptionNews"></param>
        /// <returns></returns>
        public static ReceptionViewModel ConvertToReceptionViewModel(ReceptionNew x)
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
                    RangeDetail = rt.LabReceiverRangeDetail != null ? ConvertToRangeDetailViewModel(rt.LabReceiverRangeDetail) : null,
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

            return receptionViewModel;
        }

        /// <summary>
        /// کانورت لیست به مدل
        /// </summary>
        /// <param name="receptionNews"></param>
        /// <returns></returns>
        public static List<ReceptionViewModel> ConvertModel(List<ReceptionNew> receptionNews)
        {
            return receptionNews.Select(x => new ReceptionViewModel()
            {
                SourceReceptId = x.chrSourceReceptId,
                SourceLabId = x.intSourceLabId,
                SourceSendReceptDate = x.chrSourceSendReceptDate.TrimEnd(),
                Age = x.tinAge,
                AgeType = x.chrAgeType,
                FirstName = x.vchFname,
                LastName = x.vchLName,
                Gender = x.bitSex,
                PreviousRecords = x.vchPrevRec,
                SendFromSite = x.bitFromSite,
                IsUrgent = x.bitEmg,
                NIC = x.vchNIC,
                Mobile = x.vchMobile,
                DoctorCode = x.DocCode,
                ReceptTests = x.ReceptTests.Select(rt => new ReceptTestViewModel()
                {
                    Id = rt.ID,
                    SourceLabId = rt.intSourceLabId,
                    SourceReceptId = rt.chrSourceReceptId,
                    SourceSendDate = rt.chrSourceSendDate,
                    TargetLabId = rt.intTargetLabId,
                    SourceCPN = rt.vchSourceCPN,
                    SourceTestName = rt?.vchSourceTestName?.Trim(),
                    IsUrgent = rt.bitEmg,
                    Result = rt.vchResult,
                    SourceTestId = rt.SourceTestId,
                    SourceApprovePrice = rt.SourceApprovePrice,
                    TargetApprovePrice = rt.TargetApprovePrice,
                    Tracking = rt.Tracking,
                }).ToList()
            }).ToList();
        }

        /// <summary>
        /// Covert To LabReceiverRangeDetail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static LabReceiverRangeDetailViewModel ConvertToRangeDetailViewModel(LabReceiverRangeDetail model)
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

        #endregion



        public static string ReverseReplaceChar(string result)
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
    }
}
