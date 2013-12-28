using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_Finder.Models
{
    public class FriendRoot
    {
        public List<Friend> data { get; set; }
    }

    public class Friend
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string pic_big { get; set; }
    }

}
