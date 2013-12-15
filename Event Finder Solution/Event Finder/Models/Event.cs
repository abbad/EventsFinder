using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Bing.Maps;

namespace Event_Finder.Models
{

    public class Event
    {
        public Event(){
           
        }

        public String name { get; set; }
        public String eid { get; set; }
        
        public String description { get; set; }
        public Dictionary<string, string> venue {get; set;}

        public Location Location { get; set; }
       
        
    }

}
