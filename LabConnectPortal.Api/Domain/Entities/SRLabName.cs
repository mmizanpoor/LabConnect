using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabConnectPortal.Api.Domain.Entities
{
    [Table("SR-LabName")]
    public class SRLabName
    {
        [Key]
        public int intLabId { get; set; }
        public int? intLabIdNew { get; set; }
        public string? vchLabName { get; set; }
    }
}

