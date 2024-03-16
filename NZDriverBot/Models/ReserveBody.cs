using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class ReserveBody
    {
        public string applicationId { get; set; }
        public string applicationType { get; set; }
        public string hasAdvancedDriverCertificate { get; set; }
        public bool isReschedule { get; set; }
        public string licenceClass { get; set; }
        public string siteId { get; set; }
        public string stage { get; set; }
        public string testType { get; set; }
        public string when { get; set; }

    }
}
