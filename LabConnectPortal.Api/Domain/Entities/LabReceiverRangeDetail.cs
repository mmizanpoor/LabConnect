using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    public class LabReceiverRangeDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long TargetLabId { get; set; }

        [Required]
        public long TargetRangeDetailId { get; set; }

        [Required]
        public int FromAgeScale { get; set; }

        [Required]
        public int FromAge { get; set; }

        [Required]
        public int ToAgeScale { get; set; }

        [Required]
        public int ToAge { get; set; }

        [Required]
        public int RangeSpecificationType { get; set; }

        [StringLength(500)]
        public string? BorderLineText { get; set; }

        [StringLength(500)]
        public string? CriticalLowerLimitText { get; set; }

        [Required]
        public float CriticalLowerLimitValue { get; set; }

        [StringLength(500)]
        public string? CriticalUpperLimitText { get; set; }

        [Required]
        public float CriticalUpperLimitValue { get; set; }

        [Required]
        public float MaxCriticalValue { get; set; }

        [Required]
        public float MaxNormalValue { get; set; }

        [Required]
        public float MaxPossibleValue { get; set; }

        [Required]
        public float MaxWarningValue { get; set; }

        [Required]
        public float MinCriticalValue { get; set; }

        [Required]
        public float MinNormalValue { get; set; }

        [Required]
        public float MinPossibleValue { get; set; }

        [Required]
        public float MinWarningValue { get; set; }

        [StringLength(500)]
        public string? NormalText { get; set; }

        [StringLength(500)]
        public string? WarningLowerLimitText { get; set; }

        [Required]
        public float WarningLowerLimitValue { get; set; }

        [StringLength(500)]
        public string? WarningUpperLimitText { get; set; }

        [Required]
        public float WarningUpperLimitValue { get; set; }

        [StringLength(200)]
        public string? UnitDesc { get; set; }

        [StringLength(500)]
        public string? KitTitle { get; set; }

        public string? Method { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        //Navigate 
        public List<ReceptTestNew>? ReceptTestNews { get; set; }
    }

    public enum NormalRangeStatus
    {
        Critical = 1,
        Warning = 2,
        Normal = 3,
        LowNormal = 4,
        HeighNormal = 5
    }
}

