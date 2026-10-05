using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LabConnectPortal.Domain
{
    [Table("TestsJoze3")]
    [Keyless]
    public class TestJoze3
    {
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
