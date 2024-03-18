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
        public required string mobilePhone { get; set; }
        public bool mobilePhoneExists { get; set; }
        public required string otherPhone { get; set; }
        public bool otherPhoneExists { get; set; }
        public required string emailAddress { get; set; }
        public bool emailAddressExists { get; set; }
    }
}
