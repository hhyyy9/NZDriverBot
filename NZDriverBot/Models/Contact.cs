using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class Contact
    {
        public int driverId { get; set; }
        public string mobilePhone { get; set; }
        public bool mobilePhoneExists { get; set; }
        public string otherPhone { get; set; }
        public bool otherPhoneExists { get; set; }
        public string emailAddress { get; set; }
        public bool emailAddressExists { get; set; }
    }
}
