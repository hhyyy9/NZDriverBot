using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class BookingDetailApplication
    {
        public int applicationId { get; set; }
        public string bookingId { get; set; }
        public string testType { get; set; }
        public string applicationType { get; set; }
        public string entitlementType { get; set; }
        public string entitlementValue { get; set; }
        public string stage { get; set; }
        public bool isEligible { get; set; }
        public double applicationFee { get; set; }
        public DateTime earliestDate { get; set; }
        public bool hasFutureStartDate { get; set; }
    }
}
