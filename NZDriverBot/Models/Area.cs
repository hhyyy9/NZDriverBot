using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class Area
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Location> Locations { get; set; }
    }
}
