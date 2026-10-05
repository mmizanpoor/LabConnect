using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Api.Domain.Entities
{
    [Table("TestsNew99")]
    public class TestECL
    {
        [Key]
        public long Id { get; set; }
        public string vchTestCode { get; set; }
        public string vchCPN { get; set; }
        public string nvcTestName { get; set; }
        public string vchK { get; set; }
        public string vchK2 { get; set; }
        public string vchTestPrice { get; set; }
        public string vchTestPriceNew { get; set; }
        public string vchTestPriceDiss { get; set; }
        public int intVersion { get; set; }
        public bool bitActive { get; set; }
    }
}

