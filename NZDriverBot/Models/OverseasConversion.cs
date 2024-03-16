using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class OverseasConversion
    {
        public int applicationId { get; set; }
        public string testType { get; set; }
        public string testDescription { get; set; }
        public string applicationType { get; set; }
        public string entitlementType { get; set; }
        public string entitlementValue { get; set; }
        public double fees { get; set; }
        public string entitlement { get; set; }
        public string entitlementDescription { get; set; }
        public DateTime earliestDate { get; set; }
        public bool hasFutureStartDate { get; set; }
        public bool requiresAdvancedDrivingCertificate { get; set; }
        public string stage { get; set; }
    }
}
