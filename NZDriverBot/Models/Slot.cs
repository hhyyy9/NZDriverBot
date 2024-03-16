using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    internal class Slot
    {
        public string siteId { get; set; }
        public int capacity { get; set; }
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string displayTime { get; set; }
        public int durationInMins { get; set; }
        public bool isMorning { get; set; }
    }
}
