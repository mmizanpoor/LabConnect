namespace LabConnectPortal.Infra.ViewModels
{
    public class LabReceiverRangeDetailViewModel
    {
        public long? Id { get; set; }
        public long TargetLabId { get; set; }
        public long TargetRangeDetailId { get; set; }
        public int FromAgeScale { get; set; }
        public int FromAge { get; set; }
        public int ToAgeScale { get; set; }
        public int ToAge { get; set; }
        public int RangeSpecificationType { get; set; }
        public string? BorderLineText { get; set; }
        public float MinPossibleValue { get; set; }
        public float MaxPossibleValue { get; set; }
        public float MinNormalValue { get; set; }
        public float MaxNormalValue { get; set; }
        public float MinWarningValue { get; set; }
        public float MaxWarningValue { get; set; }
        public float MinCriticalValue { get; set; }
        public float MaxCriticalValue { get; set; }
        public string? NormalText { get; set; }
        public float WarningLowerLimitValue { get; set; }
        public float WarningUpperLimitValue { get; set; }
        public string? WarningLowerLimitText { get; set; }
        public string? WarningUpperLimitText { get; set; }
        public float CriticalLowerLimitValue { get; set; }
        public float CriticalUpperLimitValue { get; set; }
        public string? CriticalLowerLimitText { get; set; }
        public string? CriticalUpperLimitText { get; set; }
        public string? UnitDesc { get; set; }
        public string? KitTitle { get; set; }
        public string? Method { get; set; }
        public bool IsDeleted { get; set; }
    }
}
