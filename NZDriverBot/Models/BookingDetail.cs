using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class BookingDetail
    {
        public string status { get; set; }
        public DateTime date { get; set; }
        public string test { get; set; }
        public string site { get; set; }
        public string address { get; set; }
        public bool isConfirmed { get; set; }
        public int durationInMinutes { get; set; }
        public BookingDetailFee fee { get; set; }
        public object application { get; set; }
        public bool canReschedule { get; set; }
        public bool canCancel { get; set; }
        public string testType { get; set; }
        public string testCategory { get; set; }
        public string id { get; set; }
    }
}
