using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NZDriverBot.Models
{
    public class Site
    {
        public string Id { get; set; }
        public string LocationId { get; set; }
        public string AreaId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double AgentId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }
    }
}
