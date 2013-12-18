using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Finder.Models
{
    public class RSVP
    {
        public string rsvp_status { get; set; }
    }

    public class RootObject
    {
        public List<RSVP> data { get; set; }
    }
}
